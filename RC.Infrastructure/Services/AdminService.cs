using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RC.Application.DTOs.Admin;
using RC.Application.DTOs.Common;
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
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<AppUser> _userManager;

        public AdminService(IUnitOfWork unitOfWork, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<ApiResponse<DashboardStatsDto>> GetDashboardStatsAsync()
        {
            try
            {
                var stats = new DashboardStatsDto();

                // Car statistics
                var allCars = await _unitOfWork.Cars.GetAllAsync();
                stats.TotalCars = allCars.Count();
                stats.AvailableCars = allCars.Count(c => c.Status == CarStatus.Available);
                stats.RentedCars = allCars.Count(c => c.Status == CarStatus.Rented);

                // Booking statistics
                var allBookings = await _unitOfWork.Bookings.GetAllAsync();
                stats.TotalBookings = allBookings.Count();
                stats.PendingBookings = allBookings.Count(b => b.Status == BookingStatus.Pending);
                stats.ConfirmedBookings = allBookings.Count(b => b.Status == BookingStatus.Confirmed);
                stats.CompletedBookings = allBookings.Count(b => b.Status == BookingStatus.Completed);

                // User statistics
                stats.TotalUsers = await _userManager.Users.CountAsync();
                stats.TotalDrivers = await _unitOfWork.Drivers.CountAsync(d => d.IsApproved);

                // Revenue statistics
                stats.TotalRevenue = await _unitOfWork.Payments.GetTotalRevenueAsync();

                var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                stats.MonthlyRevenue = await _unitOfWork.Payments.GetTotalRevenueAsync(startOfMonth);

                var today = DateTime.UtcNow.Date;
                stats.TodayRevenue = await _unitOfWork.Payments.GetTotalRevenueAsync(today);

                // Revenue chart (last 7 days)
                stats.RevenueChart = await GetRevenueChartDataAsync(7);

                // Booking status chart
                stats.BookingStatusChart = new List<BookingStatusChartDto>
            {
                new() { Status = "Pending", Count = stats.PendingBookings },
                new() { Status = "Confirmed", Count = stats.ConfirmedBookings },
                new() { Status = "Completed", Count = stats.CompletedBookings },
                new() { Status = "Cancelled", Count = allBookings.Count(b => b.Status == BookingStatus.Cancelled) }
            };

                // Popular cars
                stats.PopularCars = await GetPopularCarsAsync(5);

                // Recent bookings
                stats.RecentBookings = await GetRecentBookingsAsync(10);

                return ApiResponse<DashboardStatsDto>.SuccessResponse(stats);
            }
            catch (Exception ex)
            {
                return ApiResponse<DashboardStatsDto>.ErrorResponse($"Error retrieving dashboard stats: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResponse<AdminUserDto>>> GetAllUsersAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            try
            {
                var query = _userManager.Users.Where(u => !u.IsDeleted);

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(u =>
                        u.FirstName.Contains(searchTerm) ||
                        u.LastName.Contains(searchTerm) ||
                        u.Email!.Contains(searchTerm)
                    );
                }

                var totalCount = await query.CountAsync();
                var users = await query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var userDtos = new List<AdminUserDto>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var bookings = await _unitOfWork.Bookings.GetUserBookingsAsync(user.Id);

                    userDtos.Add(new AdminUserDto
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email!,
                        PhoneNumber = user.PhoneNumber ?? "",
                        IsVerified = user.IsVerified,
                        IsDeleted = user.IsDeleted,
                        CreatedAt = user.CreatedAt,
                        Roles = roles.ToList(),
                        TotalBookings = bookings.Count(),
                        TotalSpent = bookings
                            .Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Completed)
                            .Sum(b => b.TotalAmount)
                    });
                }

                var pagedResponse = new PagedResponse<AdminUserDto>(userDtos, totalCount, pageNumber, pageSize);
                return ApiResponse<PagedResponse<AdminUserDto>>.SuccessResponse(pagedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<AdminUserDto>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AdminUserDto>> GetUserByIdAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<AdminUserDto>.ErrorResponse("User not found");
                }

                var roles = await _userManager.GetRolesAsync(user);
                var bookings = await _unitOfWork.Bookings.GetUserBookingsAsync(userId);

                var userDto = new AdminUserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email!,
                    PhoneNumber = user.PhoneNumber ?? "",
                    IsVerified = user.IsVerified,
                    IsDeleted = user.IsDeleted,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList(),
                    TotalBookings = bookings.Count(),
                    TotalSpent = bookings
                        .Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Completed)
                        .Sum(b => b.TotalAmount)
                };

                return ApiResponse<AdminUserDto>.SuccessResponse(userDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<AdminUserDto>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> UpdateUserRolesAsync(UpdateUserRolesDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return ApiResponse<string>.ErrorResponse("User not found");
                }

                var currentRoles = await _userManager.GetRolesAsync(user);

                // Remove all current roles
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    return ApiResponse<string>.ErrorResponse("Failed to remove current roles");
                }

                // Add new roles
                var addResult = await _userManager.AddToRolesAsync(user, request.Roles);
                if (!addResult.Succeeded)
                {
                    return ApiResponse<string>.ErrorResponse("Failed to add new roles");
                }

                return ApiResponse<string>.SuccessResponse("", "User roles updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> BanUserAsync(BanUserDto request)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    return ApiResponse<string>.ErrorResponse("User not found");
                }

                user.IsDeleted = true;
                user.DeletedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return ApiResponse<string>.ErrorResponse("Failed to ban user");
                }

                // Cancel all pending bookings
                var userBookings = await _unitOfWork.Bookings.GetUserBookingsAsync(request.UserId);
                var pendingBookings = userBookings.Where(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed);

                foreach (var booking in pendingBookings)
                {
                    booking.Status = BookingStatus.Cancelled;
                    booking.CancellationReason = $"User banned: {request.Reason}";
                    booking.CancelledAt = DateTime.UtcNow;
                    await _unitOfWork.Bookings.UpdateAsync(booking);
                }

                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse("", "User banned successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> UnbanUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ApiResponse<string>.ErrorResponse("User not found");
                }

                user.IsDeleted = false;
                user.DeletedAt = null;

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return ApiResponse<string>.ErrorResponse("Failed to unban user");
                }

                return ApiResponse<string>.SuccessResponse("", "User unbanned successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResponse<RecentBookingDto>>> GetAllBookingsAsync(
            int pageNumber,
            int pageSize,
            string? status = null)
        {
            try
            {
                IQueryable<Booking> query = _unitOfWork.Bookings
                    .GetQueryableNoTracking()
                    .Include(b => b.User)
                    .Include(b => b.Car);

                if (!string.IsNullOrEmpty(status) &&
                    Enum.TryParse<BookingStatus>(status, out var bookingStatus))
                {
                    query = query.Where(b => b.Status == bookingStatus);
                }

                var totalCount = await query.CountAsync();

                var bookings = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var bookingDtos = bookings.Select(b => new RecentBookingDto
                {
                    Id = b.Id,
                    BookingNumber = b.BookingNumber,
                    UserName = $"{b.User.FirstName} {b.User.LastName}",
                    CarName = $"{b.Car.Brand} {b.Car.Model}",
                    StartDate = b.StartDate,
                    TotalAmount = b.TotalAmount,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt
                }).ToList();

                var pagedResponse =
                    new PagedResponse<RecentBookingDto>(bookingDtos, totalCount, pageNumber, pageSize);

                return ApiResponse<PagedResponse<RecentBookingDto>>.SuccessResponse(pagedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<RecentBookingDto>>
                    .ErrorResponse($"Error: {ex.Message}");
            }
        }


        public async Task<ApiResponse<string>> ApproveBookingAsync(Guid bookingId)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return ApiResponse<string>.ErrorResponse("Booking not found");
                }

                booking.Status = BookingStatus.Confirmed;
                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse("", "Booking approved successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> RejectBookingAsync(Guid bookingId, string reason)
        {
            try
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
                if (booking == null)
                {
                    return ApiResponse<string>.ErrorResponse("Booking not found");
                }

                booking.Status = BookingStatus.Rejected;
                booking.CancellationReason = reason;
                booking.CancelledAt = DateTime.UtcNow;

                await _unitOfWork.Bookings.UpdateAsync(booking);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse("", "Booking rejected");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<PagedResponse<AdminDriverDto>>> GetAllDriversAsync(int pageNumber, int pageSize)
        {
            try
            {
                var (drivers, totalCount) = await _unitOfWork.Drivers.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    includeProperties: "User"
                );

                var driverDtos = drivers.Select(d => new AdminDriverDto
                {
                    Id = d.Id,
                    UserId = d.UserId,
                    FullName = $"{d.User.FirstName} {d.User.LastName}",
                    Email = d.User.Email!,
                    PhoneNumber = d.User.PhoneNumber ?? "",
                    LicenseNumber = d.LicenseNumber,
                    LicenseExpiryDate = d.LicenseExpiryDate,
                    YearsOfExperience = d.YearsOfExperience,
                    AverageRating = d.AverageRating,
                    TotalTrips = d.TotalTrips,
                    Status = d.Status,
                    IsApproved = d.IsApproved,
                    IsVerified = d.IsVerified,
                    CreatedAt = d.CreatedAt
                }).ToList();

                var pagedResponse = new PagedResponse<AdminDriverDto>(driverDtos, totalCount, pageNumber, pageSize);
                return ApiResponse<PagedResponse<AdminDriverDto>>.SuccessResponse(pagedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<AdminDriverDto>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> ApproveDriverAsync(ApproveDriverDto request)
        {
            try
            {
                var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);
                if (driver == null)
                {
                    return ApiResponse<string>.ErrorResponse("Driver not found");
                }

                driver.IsApproved = request.IsApproved;
                driver.ApprovedAt = DateTime.UtcNow;

                await _unitOfWork.Drivers.UpdateAsync(driver);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse("", $"Driver {(request.IsApproved ? "approved" : "rejected")} successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> VerifyDriverAsync(Guid driverId)
        {
            try
            {
                var driver = await _unitOfWork.Drivers.GetByIdAsync(driverId);
                if (driver == null)
                {
                    return ApiResponse<string>.ErrorResponse("Driver not found");
                }

                driver.IsVerified = true;
                await _unitOfWork.Drivers.UpdateAsync(driver);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse("", "Driver verified successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<RevenueReportDto>> GetRevenueReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var payments = await _unitOfWork.Payments.GetQueryableNoTracking()
                    .Include(p => p.Booking)
                        .ThenInclude(b => b.Car)
                    .Where(p => p.Status == PaymentStatus.Completed &&
                               p.PaidAt >= startDate && p.PaidAt <= endDate)
                    .ToListAsync();

                var report = new RevenueReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalRevenue = payments.Sum(p => p.Amount),
                    TotalBookings = payments.Count,
                    AverageBookingValue = payments.Any() ? payments.Average(p => p.Amount) : 0
                };

                // Daily breakdown
                report.DailyBreakdown = payments
                    .GroupBy(p => p.PaidAt!.Value.Date)
                    .Select(g => new DailyRevenueDto
                    {
                        Date = g.Key,
                        Revenue = g.Sum(p => p.Amount),
                        BookingCount = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                // Car breakdown
                report.CarBreakdown = payments
                    .GroupBy(p => new { p.Booking.CarId, CarName = $"{p.Booking.Car.Brand} {p.Booking.Car.Model}" })
                    .Select(g => new CarRevenueDto
                    {
                        CarId = g.Key.CarId,
                        CarName = g.Key.CarName,
                        Revenue = g.Sum(p => p.Amount),
                        BookingCount = g.Count(),
                        TotalDays = g.Sum(p => p.Booking.TotalDays)
                    })
                    .OrderByDescending(c => c.Revenue)
                    .ToList();

                return ApiResponse<RevenueReportDto>.SuccessResponse(report);
            }
            catch (Exception ex)
            {
                return ApiResponse<RevenueReportDto>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<BookingReportDto>> GetBookingReportAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var bookings = await _unitOfWork.Bookings.GetQueryableNoTracking()
                    .Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate)
                    .ToListAsync();

                var report = new BookingReportDto
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalBookings = bookings.Count,
                    CompletedBookings = bookings.Count(b => b.Status == BookingStatus.Completed),
                    CancelledBookings = bookings.Count(b => b.Status == BookingStatus.Cancelled)
                };

                report.CompletionRate = report.TotalBookings > 0
                    ? (decimal)report.CompletedBookings / report.TotalBookings * 100
                    : 0;

                report.CancellationRate = report.TotalBookings > 0
                    ? (decimal)report.CancelledBookings / report.TotalBookings * 100
                    : 0;

                // Trend
                report.Trend = bookings
                    .GroupBy(b => b.CreatedAt.Date)
                    .Select(g => new BookingTrendDto
                    {
                        Date = g.Key,
                        TotalBookings = g.Count(),
                        Completed = g.Count(b => b.Status == BookingStatus.Completed),
                        Cancelled = g.Count(b => b.Status == BookingStatus.Cancelled)
                    })
                    .OrderBy(t => t.Date)
                    .ToList();

                return ApiResponse<BookingReportDto>.SuccessResponse(report);
            }
            catch (Exception ex)
            {
                return ApiResponse<BookingReportDto>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<PopularCarDto>>> GetPopularCarsReportAsync(int topCount = 10)
        {
            try
            {
                var popularCars = await GetPopularCarsAsync(topCount);
                return ApiResponse<List<PopularCarDto>>.SuccessResponse(popularCars);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<PopularCarDto>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<SystemSettingsDto>>> GetAllSettingsAsync()
        {
            try
            {
                var settings = await _unitOfWork.Settings.GetAllAsync();

                var settingDtos = settings.Select(s => new SystemSettingsDto
                {
                    Key = s.Key,
                    Value = s.Value,
                    Description = s.Description,
                    Category = s.Category
                }).ToList();

                return ApiResponse<List<SystemSettingsDto>>.SuccessResponse(settingDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SystemSettingsDto>>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> UpdateSettingAsync(UpdateSystemSettingDto request)
        {
            try
            {
                var setting = await _unitOfWork.Settings.FirstOrDefaultAsync(s => s.Key == request.Key);

                if (setting == null)
                {
                    return ApiResponse<string>.ErrorResponse("Setting not found");
                }

                setting.Value = request.Value;
                await _unitOfWork.Settings.UpdateAsync(setting);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse("", "Setting updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error: {ex.Message}");
            }
        }

        // Private helper methods
        private async Task<List<RevenueChartDataDto>> GetRevenueChartDataAsync(int days)
        {
            var chartData = new List<RevenueChartDataDto>();
            var startDate = DateTime.UtcNow.Date.AddDays(-days);

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var revenue = await _unitOfWork.Payments.GetTotalRevenueAsync(date, date.AddDays(1));

                chartData.Add(new RevenueChartDataDto
                {
                    Date = date.ToString("MMM dd"),
                    Amount = revenue
                });
            }

            return chartData;
        }

        private async Task<List<PopularCarDto>> GetPopularCarsAsync(int count)
        {
            var bookings = await _unitOfWork.Bookings.GetQueryableNoTracking()
                .Include(b => b.Car)
                .Include(b => b.Payment)
                .Where(b => b.Status != BookingStatus.Cancelled)
                .ToListAsync();

            var popularCars = bookings
                .GroupBy(b => new { b.CarId, b.Car.Brand, b.Car.Model, b.Car.MainImageUrl })
                .Select(g => new PopularCarDto
                {
                    Id = g.Key.CarId,
                    Brand = g.Key.Brand,
                    Model = g.Key.Model,
                    ImageUrl = g.Key.MainImageUrl,
                    BookingCount = g.Count(),
                    TotalRevenue = g.Where(b => b.Payment != null && b.Payment.Status == PaymentStatus.Completed)
                                    .Sum(b => b.TotalAmount)
                })
                .OrderByDescending(c => c.BookingCount)
                .Take(count)
                .ToList();

            return popularCars;
        }

        private async Task<List<RecentBookingDto>> GetRecentBookingsAsync(int count)
        {
            var bookings = await _unitOfWork.Bookings.GetQueryableNoTracking()
                .Include(b => b.User)
                .Include(b => b.Car)
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();

            return bookings.Select(b => new RecentBookingDto
            {
                Id = b.Id,
                BookingNumber = b.BookingNumber,
                UserName = $"{b.User.FirstName} {b.User.LastName}",
                CarName = $"{b.Car.Brand} {b.Car.Model}",
                StartDate = b.StartDate,
                TotalAmount = b.TotalAmount,
                Status = b.Status,
                CreatedAt = b.CreatedAt
            }).ToList();
        }
    }
}
