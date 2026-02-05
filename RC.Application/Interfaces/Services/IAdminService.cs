using RC.Application.DTOs.Admin;
using RC.Application.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Application.Interfaces.Services
{
    public interface IAdminService
    {
        // Dashboard
        Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync();

        // User Management
        Task<ApiResponse<PagedResponse<AdminUserDto>>> GetAllUsersAsync(int pageNumber, int pageSize, string? searchTerm = null);
        Task<ApiResponse<AdminUserDto>> GetUserByIdAsync(string userId);
        Task<ApiResponse<string>> UpdateUserRolesAsync(UpdateUserRolesDto request);
        Task<ApiResponse<string>> BanUserAsync(BanUserDto request);
        Task<ApiResponse<string>> UnbanUserAsync(string userId);

        // Booking Management
        Task<ApiResponse<PagedResponse<RecentBookingDto>>> GetAllBookingsAsync(int pageNumber, int pageSize, string? status = null);
        Task<ApiResponse<string>> ApproveBookingAsync(Guid bookingId);
        Task<ApiResponse<string>> RejectBookingAsync(Guid bookingId, string reason);

        // Driver Management
        Task<ApiResponse<PagedResponse<AdminDriverDto>>> GetAllDriversAsync(int pageNumber, int pageSize);
        Task<ApiResponse<string>> ApproveDriverAsync(ApproveDriverDto request);
        Task<ApiResponse<string>> VerifyDriverAsync(Guid driverId);

        // Reports
        Task<ApiResponse<RevenueReportDto>> GetRevenueReportAsync(DateTime startDate, DateTime endDate);
        Task<ApiResponse<BookingReportDto>> GetBookingReportAsync(DateTime startDate, DateTime endDate);
        Task<ApiResponse<List<PopularCarDto>>> GetPopularCarsReportAsync(int topCount = 10);

        // Settings
        Task<ApiResponse<List<SystemSettingsDto>>> GetAllSettingsAsync();
        Task<ApiResponse<string>> UpdateSettingAsync(UpdateSystemSettingDto request);
    }

}
