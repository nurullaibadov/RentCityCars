using RC.Application.DTOs.Auth;
using RC.Application.DTOs.Booking;
using RC.Application.DTOs.Car;
using RC.Application.DTOs.Common;
using RC.Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<ApiResponse<string>> ForgotPasswordAsync(ForgotPasswordRequestDto request);
        Task<ApiResponse<string>> ResetPasswordAsync(ResetPasswordRequestDto request);
        Task<ApiResponse<string>> VerifyEmailAsync(VerifyEmailRequestDto request);
        Task<ApiResponse<string>> ChangePasswordAsync(string userId, ChangePasswordRequestDto request);
        Task<AuthResponseDto> RefreshTokenAsync(string refreshToken);
    }

    public interface ICarService
    {
        Task<ApiResponse<PagedResponse<CarDto>>> GetAllCarsAsync(CarSearchRequestDto request);
        Task<ApiResponse<CarDetailsDto>> GetCarByIdAsync(Guid id);
        Task<ApiResponse<IEnumerable<CarDto>>> GetFeaturedCarsAsync(int count = 10);
        Task<ApiResponse<IEnumerable<CarDto>>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate);
        Task<ApiResponse<CarDto>> CreateCarAsync(CreateCarRequestDto request);
        Task<ApiResponse<CarDto>> UpdateCarAsync(UpdateCarRequestDto request);
        Task<ApiResponse<string>> DeleteCarAsync(Guid id);
        Task<ApiResponse<bool>> CheckAvailabilityAsync(Guid carId, DateTime startDate, DateTime endDate);
    }

    public interface IBookingService
    {
        Task<ApiResponse<BookingDetailsDto>> CreateBookingAsync(string userId, CreateBookingRequestDto request);
        Task<ApiResponse<IEnumerable<BookingDto>>> GetMyBookingsAsync(string userId);
        Task<ApiResponse<BookingDetailsDto>> GetBookingByIdAsync(Guid id);
        Task<ApiResponse<BookingDetailsDto>> GetBookingByNumberAsync(string bookingNumber);
        Task<ApiResponse<BookingCalculationDto>> CalculateBookingCostAsync(CreateBookingRequestDto request);
        Task<ApiResponse<string>> CancelBookingAsync(Guid id, string userId);
        Task<ApiResponse<string>> UpdateBookingStatusAsync(UpdateBookingStatusRequestDto request);
    }

    public interface IPaymentService
    {
        Task<PaymentResponseDto> ProcessPaymentAsync(string userId, ProcessPaymentRequestDto request);
        Task<ApiResponse<PaymentDto>> GetPaymentByIdAsync(Guid id);
        Task<ApiResponse<IEnumerable<PaymentDto>>> GetUserPaymentsAsync(string userId);
        Task<ApiResponse<string>> GenerateInvoiceAsync(Guid paymentId);
    }

    public interface IUserService
    {
        Task<ApiResponse<UserDto>> GetProfileAsync(string userId);
        Task<ApiResponse<UserDto>> UpdateProfileAsync(string userId, UpdateProfileRequestDto request);
        Task<ApiResponse<string>> UploadProfileImageAsync(string userId, Stream imageStream, string fileName);
    }

    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendWelcomeEmailAsync(string to, string userName);
        Task SendBookingConfirmationEmailAsync(string to, BookingDetailsDto booking);
        Task SendPasswordResetEmailAsync(string to, string resetToken);
        Task SendEmailVerificationAsync(string to, string verificationToken);
    }

    public interface INotificationService
    {
        Task SendNotificationAsync(string userId, string title, string message);
        Task<ApiResponse<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(string userId);
        Task<ApiResponse<string>> MarkAsReadAsync(Guid notificationId);
    }

    public class UpdateProfileRequestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
    }

    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
