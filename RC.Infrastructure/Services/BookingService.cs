using Microsoft.EntityFrameworkCore;
using RC.Application.DTOs.Booking;
using RC.Application.DTOs.Common;
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
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;

        public BookingService(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _notificationService = notificationService;
        }

        public async Task<ApiResponse<BookingDetailsDto>> CreateBookingAsync(string userId, CreateBookingRequestDto request)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // Validate car exists and is available
                var car = await _unitOfWork.Cars.GetByIdAsync(request.CarId);
                if (car == null)
                {
                    return ApiResponse<BookingDetailsDto>.ErrorResponse("Car not found");
                }

                // Check if car is available for the dates
                var hasOverlap = await _unitOfWork.Bookings.HasOverlappingBookingsAsync(
                    request.CarId,
                    request.StartDate,
                    request.EndDate
                );

                if (hasOverlap)
                {
                    return ApiResponse<BookingDetailsDto>.ErrorResponse("Car is not available for selected dates");
                }

                // Calculate costs
                var calculation = await CalculateBookingCostInternalAsync(request, car);

                // Create booking
                var booking = new Booking
                {
                    UserId = userId,
                    CarId = request.CarId,
                    BookingNumber = GenerateBookingNumber(),
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    TotalDays = calculation.TotalDays,
                    PricePerDay = calculation.PricePerDay,
                    SubTotal = calculation.SubTotal,
                    TaxAmount = calculation.TaxAmount,
                    DiscountAmount = calculation.DiscountAmount,
                    DepositAmount = calculation.DepositAmount,
                    TotalAmount = calculation.TotalAmount,
                    PickupLocationId = request.PickupLocationId,
                    ReturnLocationId = request.ReturnLocationId,
                    NeedsDriver = request.NeedsDriver,
                    WithInsurance = request.WithInsurance,
                    WithGPS = request.WithGPS,
                    WithChildSeat = request.WithChildSeat,
                    AdditionalDrivers = request.AdditionalDrivers,
                    ContactPhone = request.ContactPhone,
                    ContactEmail = request.ContactEmail,
                    Notes = request.Notes,
                    Status = BookingStatus.Pending
                };

                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                // Send notification
                await _notificationService.SendNotificationAsync(
                    userId,
                    "Booking Created",
                    $"Your booking #{booking.BookingNumber} has been created successfully"
                );

                await _unitOfWork.CommitTransactionAsync();

                var bookingDto = await GetBookingDetailsDtoAsync(booking.Id);

                return ApiResponse<BookingDetailsDto>.SuccessResponse(
                    bookingDto.Data!,
                    "Booking created successfully"
                );
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                return ApiResponse<BookingDetailsDto>.ErrorResponse($"Error creating booking: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<BookingDto>>> GetMyBookingsAsync(string userId)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetUserBookingsAsync(userId);

                var bookingDtos = bookings.Select(b => new BookingDto
                {
                    Id = b.Id,
                    BookingNumber = b.BookingNumber,
                    StartDate = b.StartDate,
                    EndDate = b.EndDate,
                    TotalDays = b.TotalDays,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    CarId = b.CarId,
                    CarBrand = b.Car.Brand,
                    CarModel = b.Car.Model,
                    CarImageUrl = b.Car.MainImageUrl,
                    PickupLocationName = b.PickupLocation.Name,
                    ReturnLocationName = b.ReturnLocation.Name,
                    DriverName = b.AssignedDriver != null ? $"{b.AssignedDriver.User.FirstName} {b.AssignedDriver.User.LastName}" : null,
                    IsPaid = b.Payment != null && b.Payment.Status == PaymentStatus.Completed,
                    PaymentStatus = b.Payment?.Status
                });

                return ApiResponse<IEnumerable<BookingDto>>.SuccessResponse(bookingDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<BookingDto>>.ErrorResponse($"Error retrieving bookings: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BookingDetailsDto>> GetBookingByIdAsync(Guid id)
        {
            return await GetBookingDetailsDtoAsync(id);
        }

        public async Task<ApiResponse<BookingDetailsDto>> GetBookingByNumberAsync(string bookingNumber)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetBookingByNumberAsync(bookingNumber);
                if (booking == null)
                {
                    return ApiResponse<BookingDetailsDto>.ErrorResponse("Booking not found");
                }

                return await GetBookingDetailsDtoAsync(booking.Id);
            }
            catch (Exception ex)
            {
                return ApiResponse<BookingDetailsDto>.ErrorResponse($"Error retrieving booking: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BookingCalculationDto>> CalculateBookingCostAsync(CreateBookingRequestDto request)
        {
            try
            {
                var car = await _unitOfWork.Cars.GetByIdAsync(request.CarId);
                if (car == null)
                {
                    return ApiResponse<BookingCalculationDto>.ErrorResponse("Car not found");
                }

                var calculation = await CalculateBookingCostInternalAsync(request, car);

                return ApiResponse<BookingCalculationDto>.SuccessResponse(calculation);
            }
            catch (Exception ex)
            {
                return ApiResponse<BookingCalculationDto>.ErrorResponse($"Error calculating cost: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> CancelBookingAsync(Guid id, string userId)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(id);
                if (booking == null)
                {
                    return ApiResponse<string>.ErrorResponse("Booking not found");
                }

                if (booking.UserId != userId)
                {
                    return ApiResponse<string>.ErrorResponse("Unauthorized");
                }

                if (booking.Status == BookingStatus.Completed || booking.Status == BookingStatus.Cancelled)
                {
                    return ApiResponse<string>.ErrorResponse("Cannot cancel this booking");
                }

                booking.Status = BookingStatus.Cancelled;
                booking.CancelledAt = DateTime.UtcNow;

                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                // Send notification
                await _notificationService.SendNotificationAsync(
                    userId,
                    "Booking Cancelled",
                    $"Your booking #{booking.BookingNumber} has been cancelled"
                );

                return ApiResponse<string>.SuccessResponse("", "Booking cancelled successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error cancelling booking: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> UpdateBookingStatusAsync(UpdateBookingStatusRequestDto request)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(request.BookingId);
                if (booking == null)
                {
                    return ApiResponse<string>.ErrorResponse("Booking not found");
                }

                booking.Status = request.Status;
                if (!string.IsNullOrEmpty(request.Notes))
                {
                    booking.Notes = request.Notes;
                }

                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse("", "Booking status updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error updating booking: {ex.Message}");
            }
        }

        private async Task<BookingCalculationDto> CalculateBookingCostInternalAsync(CreateBookingRequestDto request, Car car)
        {
            var totalDays = (request.EndDate.Date - request.StartDate.Date).Days + 1;
            var pricePerDay = car.PricePerDay;
            var subTotal = pricePerDay * totalDays;

            // Additional costs
            decimal driverCost = request.NeedsDriver ? 50m * totalDays : 0; // $50 per day
            decimal insuranceCost = request.WithInsurance ? 10m * totalDays : 0;
            decimal gpsCost = request.WithGPS ? 5m * totalDays : 0;
            decimal childSeatCost = request.WithChildSeat ? 3m * totalDays : 0;
            decimal additionalDriversCost = request.AdditionalDrivers * 10m * totalDays;

            var allAdditionalCosts = driverCost + insuranceCost + gpsCost + childSeatCost + additionalDriversCost;

            // Tax (18%)
            var taxAmount = (subTotal + allAdditionalCosts) * 0.18m;

            // Discount
            var discountAmount = (subTotal + allAdditionalCosts) * (car.DiscountPercentage / 100);

            var totalAmount = subTotal + allAdditionalCosts + taxAmount - discountAmount;

            return new BookingCalculationDto
            {
                TotalDays = totalDays,
                PricePerDay = pricePerDay,
                SubTotal = subTotal,
                DriverCost = driverCost,
                InsuranceCost = insuranceCost,
                GPSCost = gpsCost,
                ChildSeatCost = childSeatCost,
                AdditionalDriversCost = additionalDriversCost,
                TaxAmount = taxAmount,
                DiscountAmount = discountAmount,
                DepositAmount = car.DepositAmount,
                TotalAmount = totalAmount
            };
        }

        private async Task<ApiResponse<BookingDetailsDto>> GetBookingDetailsDtoAsync(Guid bookingId)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetQueryableNoTracking()
                    .Include(b => b.Car)
                    .Include(b => b.PickupLocation)
                    .Include(b => b.ReturnLocation)
                    .Include(b => b.AssignedDriver)
                        .ThenInclude(d => d!.User)
                    .Include(b => b.Payment)
                    .FirstOrDefaultAsync(b => b.Id == bookingId);

                if (booking == null)
                {
                    return ApiResponse<BookingDetailsDto>.ErrorResponse("Booking not found");
                }

                var dto = new BookingDetailsDto
                {
                    Id = booking.Id,
                    BookingNumber = booking.BookingNumber,
                    StartDate = booking.StartDate,
                    EndDate = booking.EndDate,
                    TotalDays = booking.TotalDays,
                    PricePerDay = booking.PricePerDay,
                    SubTotal = booking.SubTotal,
                    TaxAmount = booking.TaxAmount,
                    DiscountAmount = booking.DiscountAmount,
                    DepositAmount = booking.DepositAmount,
                    TotalAmount = booking.TotalAmount,
                    Status = booking.Status,
                    CreatedAt = booking.CreatedAt,
                    CarId = booking.CarId,
                    CarBrand = booking.Car.Brand,
                    CarModel = booking.Car.Model,
                    CarImageUrl = booking.Car.MainImageUrl,
                    PickupLocationName = booking.PickupLocation.Name,
                    ReturnLocationName = booking.ReturnLocation.Name,
                    WithInsurance = booking.WithInsurance,
                    WithGPS = booking.WithGPS,
                    WithChildSeat = booking.WithChildSeat,
                    AdditionalDrivers = booking.AdditionalDrivers,
                    ContactPhone = booking.ContactPhone,
                    ContactEmail = booking.ContactEmail,
                    Notes = booking.Notes,
                    ActualPickupTime = booking.ActualPickupTime,
                    ActualReturnTime = booking.ActualReturnTime,
                    IsPaid = booking.Payment != null && booking.Payment.Status == PaymentStatus.Completed,
                    PaymentStatus = booking.Payment?.Status,
                    DriverName = booking.AssignedDriver != null
                        ? $"{booking.AssignedDriver.User.FirstName} {booking.AssignedDriver.User.LastName}"
                        : null
                };

                if (booking.Payment != null)
                {
                    dto.Payment = new PaymentDto
                    {
                        Id = booking.Payment.Id,
                        TransactionId = booking.Payment.TransactionId,
                        Amount = booking.Payment.Amount,
                        Method = booking.Payment.Method,
                        Status = booking.Payment.Status,
                        CreatedAt = booking.Payment.CreatedAt,
                        PaidAt = booking.Payment.PaidAt,
                        CardLastFourDigits = booking.Payment.CardLastFourDigits,
                        InvoiceUrl = booking.Payment.InvoiceUrl
                    };
                }

                return ApiResponse<BookingDetailsDto>.SuccessResponse(dto);
            }
            catch (Exception ex)
            {
                return ApiResponse<BookingDetailsDto>.ErrorResponse($"Error retrieving booking: {ex.Message}");
            }
        }

        private string GenerateBookingNumber()
        {
            return $"BK{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString()[..6].ToUpper()}";
        }
    }
}
