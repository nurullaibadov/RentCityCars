using Microsoft.EntityFrameworkCore;
using RC.Application.DTOs.Payment;
using RC.Application.Interfaces.Repositories;
using RC.Application.Interfaces.Services;
using RC.Domain.Entities;
using RC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public PaymentService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task<PaymentResponseDto> ProcessPaymentAsync(string userId, ProcessPaymentRequestDto request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Get booking
                var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId);
                if (booking == null)
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = "Booking not found"
                    };
                }

                if (booking.UserId != userId)
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = "Unauthorized"
                    };
                }

                // Check if already paid
                var existingPayment = await _unitOfWork.Payments.FirstOrDefaultAsync(
                    p => p.BookingId == request.BookingId && p.Status == PaymentStatus.Completed
                );

                if (existingPayment != null)
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = "Booking already paid"
                    };
                }

                // Simulate payment processing (In production: integrate with Stripe/PayPal)
                var paymentResult = await SimulatePaymentGatewayAsync(request);

                var payment = new Payment
                {
                    UserId = userId,
                    BookingId = request.BookingId,
                    TransactionId = GenerateTransactionId(),
                    PaymentReference = $"PAY-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    Amount = request.Amount,
                    Method = request.Method,
                    Status = paymentResult.Success ? PaymentStatus.Completed : PaymentStatus.Failed,
                    PaidAt = paymentResult.Success ? DateTime.UtcNow : null,
                    CardLastFourDigits = request.CardNumber?.Length >= 4 ? request.CardNumber[^4..] : null,
                    CardBrand = DetectCardBrand(request.CardNumber),
                    PaymentGatewayResponse = paymentResult.GatewayResponse,
                    FailureReason = paymentResult.FailureReason
                };

                await _unitOfWork.Payments.AddAsync(payment);

                if (paymentResult.Success)
                {
                    // Update booking status
                    booking.Status = BookingStatus.Confirmed;
                    await _unitOfWork.Bookings.UpdateAsync(booking);

                    // Update car status
                    await _unitOfWork.Cars.UpdateCarStatusAsync(booking.CarId, CarStatus.Reserved);

                    // Generate invoice
                    payment.InvoiceUrl = await GenerateInvoiceAsync(payment, booking);
                    payment.InvoiceGenerated = true;

                    // Send confirmation email
                    var bookingDetails = await _unitOfWork.Bookings.GetQueryableNoTracking()
                        .Where(b => b.Id == booking.Id)
                        .Select(b => new Application.DTOs.Booking.BookingDetailsDto
                        {
                            Id = b.Id,
                            BookingNumber = b.BookingNumber,
                            StartDate = b.StartDate,
                            EndDate = b.EndDate,
                            TotalAmount = b.TotalAmount,
                            CarBrand = b.Car.Brand,
                            CarModel = b.Car.Model,
                            CarImageUrl = b.Car.MainImageUrl,
                            PickupLocationName = b.PickupLocation.Name,
                            ReturnLocationName = b.ReturnLocation.Name
                        })
                        .FirstOrDefaultAsync();

                    if (bookingDetails != null)
                    {
                        await _emailService.SendBookingConfirmationEmailAsync(booking.ContactEmail, bookingDetails);
                    }

                    // Send notification
                    await _notificationService.SendNotificationAsync(
                        userId,
                        "Payment Successful",
                        $"Your payment of ${request.Amount:F2} has been processed successfully. Booking #{booking.BookingNumber} is confirmed."
                    );
                }
                else
                {
                    // Send failure notification
                    await _notificationService.SendNotificationAsync(
                        userId,
                        "Payment Failed",
                        $"Your payment of ${request.Amount:F2} failed. {paymentResult.FailureReason}"
                    );
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                var paymentDto = new PaymentDto
                {
                    Id = payment.Id,
                    TransactionId = payment.TransactionId,
                    Amount = payment.Amount,
                    Method = payment.Method,
                    Status = payment.Status,
                    CreatedAt = payment.CreatedAt,
                    PaidAt = payment.PaidAt,
                    CardLastFourDigits = payment.CardLastFourDigits,
                    InvoiceUrl = payment.InvoiceUrl
                };

                return new PaymentResponseDto
                {
                    Success = paymentResult.Success,
                    Message = paymentResult.Success ? "Payment processed successfully" : "Payment failed",
                    Payment = paymentDto,
                    InvoiceUrl = payment.InvoiceUrl
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return new PaymentResponseDto
                {
                    Success = false,
                    Message = $"Payment processing error: {ex.Message}"
                };
            }
        }

        public async Task<Application.DTOs.Common.ApiResponse<PaymentDto>> GetPaymentByIdAsync(Guid id)
        {
            try
            {
                var payment = await _unitOfWork.Payments.GetByIdAsync(id);
                if (payment == null)
                {
                    return Application.DTOs.Common.ApiResponse<PaymentDto>.ErrorResponse("Payment not found");
                }

                var dto = new PaymentDto
                {
                    Id = payment.Id,
                    TransactionId = payment.TransactionId,
                    Amount = payment.Amount,
                    Method = payment.Method,
                    Status = payment.Status,
                    CreatedAt = payment.CreatedAt,
                    PaidAt = payment.PaidAt,
                    CardLastFourDigits = payment.CardLastFourDigits,
                    InvoiceUrl = payment.InvoiceUrl
                };

                return Application.DTOs.Common.ApiResponse<PaymentDto>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                return Application.DTOs.Common.ApiResponse<PaymentDto>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<Application.DTOs.Common.ApiResponse<IEnumerable<PaymentDto>>> GetUserPaymentsAsync(string userId)
        {
            try
            {
                var payments = await _unitOfWork.Payments.GetUserPaymentsAsync(userId);

                var paymentDtos = payments.Select(p => new PaymentDto
                {
                    Id = p.Id,
                    TransactionId = p.TransactionId,
                    Amount = p.Amount,
                    Method = p.Method,
                    Status = p.Status,
                    CreatedAt = p.CreatedAt,
                    PaidAt = p.PaidAt,
                    CardLastFourDigits = p.CardLastFourDigits,
                    InvoiceUrl = p.InvoiceUrl
                });

                return Application.DTOs.Common.ApiResponse<IEnumerable<PaymentDto>>.SuccessResponse(paymentDtos);
            }
            catch (Exception ex)
            {
                return Application.DTOs.Common.ApiResponse<IEnumerable<PaymentDto>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<Application.DTOs.Common.ApiResponse<string>> GenerateInvoiceAsync(Guid paymentId)
        {
            try
            {
                var payment = await _unitOfWork.Payments.GetQueryableNoTracking()
                    .Include(p => p.Booking)
                        .ThenInclude(b => b.Car)
                    .Include(p => p.Booking)
                        .ThenInclude(b => b.User)
                    .FirstOrDefaultAsync(p => p.Id == paymentId);

                if (payment == null)
                {
                    return Application.DTOs.Common.ApiResponse<string>.ErrorResponse("Payment not found");
                }

                var invoiceUrl = await GenerateInvoiceAsync(payment, payment.Booking);

                payment.InvoiceUrl = invoiceUrl;
                payment.InvoiceGenerated = true;

                await _unitOfWork.Payments.UpdateAsync(payment);
                await _unitOfWork.SaveChangesAsync();

                return Application.DTOs.Common.ApiResponse<string>.SuccessResponse(invoiceUrl, "Invoice generated successfully");
            }
            catch (Exception ex)
            {
                return Application.DTOs.Common.ApiResponse<string>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // Private helper methods
        private async Task<(bool Success, string? FailureReason, string GatewayResponse)> SimulatePaymentGatewayAsync(ProcessPaymentRequestDto request)
        {
            // Simulate payment processing delay
            await Task.Delay(1000);

            // Simulate 95% success rate
            var random = new Random();
            var success = random.Next(100) < 95;

            if (success)
            {
                return (true, null, $"{{\"status\":\"success\",\"transaction_id\":\"{Guid.NewGuid()}\",\"timestamp\":\"{DateTime.UtcNow:O}\"}}");
            }
            else
            {
                var errors = new[] { "Insufficient funds", "Card declined", "Invalid card number", "Expired card" };
                var error = errors[random.Next(errors.Length)];
                return (false, error, $"{{\"status\":\"failed\",\"error\":\"{error}\",\"timestamp\":\"{DateTime.UtcNow:O}\"}}");
            }
        }

        private string GenerateTransactionId()
        {
            return $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }

        private string? DetectCardBrand(string? cardNumber)
        {
            if (string.IsNullOrEmpty(cardNumber) || cardNumber.Length < 2)
                return null;

            var firstTwo = cardNumber[..2];
            return firstTwo switch
            {
                "4" => "Visa",
                "51" or "52" or "53" or "54" or "55" => "Mastercard",
                "34" or "37" => "American Express",
                "60" or "64" or "65" => "Discover",
                _ => "Unknown"
            };
        }

        private async Task<string> GenerateInvoiceAsync(Payment payment, Booking booking)
        {
            // In production: Generate PDF invoice
            // For now, return a mock URL
            await Task.CompletedTask;
            return $"https://citycars.az/invoices/{payment.TransactionId}.pdf";
        }
    }
}
