using RC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Application.DTOs.Admin
{
    public class DashboardStatsDto
    {
        public int TotalCars { get; set; }
        public int AvailableCars { get; set; }
        public int RentedCars { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int TotalUsers { get; set; }
        public int TotalDrivers { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public List<RevenueChartDataDto> RevenueChart { get; set; } = new();
        public List<BookingStatusChartDto> BookingStatusChart { get; set; } = new();
        public List<PopularCarDto> PopularCars { get; set; } = new();
        public List<RecentBookingDto> RecentBookings { get; set; } = new();
    }

    public class RevenueChartDataDto
    {
        public string Date { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class BookingStatusChartDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class PopularCarDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class RecentBookingDto
    {
        public Guid Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string CarName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // User Management DTOs
    public class AdminUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
        public int TotalBookings { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class UpdateUserRolesDto
    {
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }

    public class BanUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    // Reports DTOs
    public class RevenueReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalBookings { get; set; }
        public decimal AverageBookingValue { get; set; }
        public List<DailyRevenueDto> DailyBreakdown { get; set; } = new();
        public List<CarRevenueDto> CarBreakdown { get; set; } = new();
    }

    public class DailyRevenueDto
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
    }

    public class CarRevenueDto
    {
        public Guid CarId { get; set; }
        public string CarName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
        public int TotalDays { get; set; }
    }

    public class BookingReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal CancellationRate { get; set; }
        public List<BookingTrendDto> Trend { get; set; } = new();
    }

    public class BookingTrendDto
    {
        public DateTime Date { get; set; }
        public int TotalBookings { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }

    // Driver Management DTOs
    public class AdminDriverDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseExpiryDate { get; set; }
        public int YearsOfExperience { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalTrips { get; set; }
        public DriverStatus Status { get; set; }
        public bool IsApproved { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ApproveDriverDto
    {
        public Guid DriverId { get; set; }
        public bool IsApproved { get; set; }
        public string? Notes { get; set; }
    }

    // Settings DTOs
    public class SystemSettingsDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = string.Empty;
    }

    public class UpdateSystemSettingDto
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
