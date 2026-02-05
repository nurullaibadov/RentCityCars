using RC.Domain.Entities;
using RC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Application.Interfaces.Repositories
{
    public interface ICarRepository : IGenericRepository<Car>
    {
        Task<IEnumerable<Car>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Car>> GetFeaturedCarsAsync(int count = 10);
        Task<IEnumerable<Car>> SearchCarsAsync(string searchTerm);
        Task<IEnumerable<Car>> GetCarsByCategoryAsync(string category);
        Task<IEnumerable<Car>> GetCarsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<bool> IsCarAvailableAsync(Guid carId, DateTime startDate, DateTime endDate);
        Task UpdateCarStatusAsync(Guid carId, CarStatus status);
    }

    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId);
        Task<IEnumerable<Booking>> GetBookingsByStatusAsync(BookingStatus status);
        Task<Booking?> GetBookingByNumberAsync(string bookingNumber);
        Task<IEnumerable<Booking>> GetUpcomingBookingsAsync();
        Task<IEnumerable<Booking>> GetActiveBookingsAsync();
        Task<IEnumerable<Booking>> GetBookingsByCarIdAsync(Guid carId);
        Task<bool> HasOverlappingBookingsAsync(Guid carId, DateTime startDate, DateTime endDate, Guid? excludeBookingId = null);
    }

    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetUserPaymentsAsync(string userId);
        Task<Payment?> GetPaymentByTransactionIdAsync(string transactionId);
        Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status);
        Task<decimal> GetTotalRevenueAsync(DateTime? startDate = null, DateTime? endDate = null);
    }

    public interface IDriverRepository : IGenericRepository<Driver>
    {
        Task<IEnumerable<Driver>> GetAvailableDriversAsync();
        Task<Driver?> GetDriverByUserIdAsync(string userId);
        Task<IEnumerable<Driver>> GetTopRatedDriversAsync(int count = 10);
        Task UpdateDriverStatusAsync(Guid driverId, DriverStatus status);
        Task UpdateDriverLocationAsync(Guid driverId, double latitude, double longitude);
    }

    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<IEnumerable<Review>> GetCarReviewsAsync(Guid carId);
        Task<IEnumerable<Review>> GetUserReviewsAsync(string userId);
        Task<IEnumerable<Review>> GetPendingReviewsAsync();
        Task<double> GetAverageRatingForCarAsync(Guid carId);
    }

    public interface ILocationRepository : IGenericRepository<Location>
    {
        Task<IEnumerable<Location>> GetActiveLocationsAsync();
        Task<Location?> GetNearestLocationAsync(double latitude, double longitude);
    }

    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(string userId);
    }

}
