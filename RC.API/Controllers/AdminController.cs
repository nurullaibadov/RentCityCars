using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RC.Application.DTOs.Admin;
using RC.Application.Interfaces.Services;

namespace RC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // ============ DASHBOARD ============

        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var result = await _adminService.GetDashboardStatsAsync();

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ============ USER MANAGEMENT ============

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? searchTerm = null)
        {
            var result = await _adminService.GetAllUsersAsync(pageNumber, pageSize, searchTerm);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("users/{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var result = await _adminService.GetUserByIdAsync(userId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPut("users/{userId}/roles")]
        public async Task<IActionResult> UpdateUserRoles(string userId, [FromBody] List<string> roles)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var request = new UpdateUserRolesDto { UserId = userId, Roles = roles };
            var result = await _adminService.UpdateUserRolesAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("users/{userId}/ban")]
        public async Task<IActionResult> BanUser(string userId, [FromBody] string reason)
        {
            var request = new BanUserDto { UserId = userId, Reason = reason };
            var result = await _adminService.BanUserAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("users/{userId}/unban")]
        public async Task<IActionResult> UnbanUser(string userId)
        {
            var result = await _adminService.UnbanUserAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ============ BOOKING MANAGEMENT ============

        [HttpGet("bookings")]
        public async Task<IActionResult> GetAllBookings([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
        {
            var result = await _adminService.GetAllBookingsAsync(pageNumber, pageSize, status);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("bookings/{bookingId}/approve")]
        public async Task<IActionResult> ApproveBooking(Guid bookingId)
        {
            var result = await _adminService.ApproveBookingAsync(bookingId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("bookings/{bookingId}/reject")]
        public async Task<IActionResult> RejectBooking(Guid bookingId, [FromBody] string reason)
        {
            var result = await _adminService.RejectBookingAsync(bookingId, reason);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ============ DRIVER MANAGEMENT ============

        [HttpGet("drivers")]
        public async Task<IActionResult> GetAllDrivers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _adminService.GetAllDriversAsync(pageNumber, pageSize);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("drivers/{driverId}/approve")]
        public async Task<IActionResult> ApproveDriver(Guid driverId, [FromBody] bool isApproved)
        {
            var request = new ApproveDriverDto
            {
                DriverId = driverId,
                IsApproved = isApproved
            };

            var result = await _adminService.ApproveDriverAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("drivers/{driverId}/verify")]
        public async Task<IActionResult> VerifyDriver(Guid driverId)
        {
            var result = await _adminService.VerifyDriverAsync(driverId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ============ REPORTS ============

        [HttpGet("reports/revenue")]
        public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate >= endDate)
                return BadRequest("Start date must be before end date");

            var result = await _adminService.GetRevenueReportAsync(startDate, endDate);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("reports/bookings")]
        public async Task<IActionResult> GetBookingReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate >= endDate)
                return BadRequest("Start date must be before end date");

            var result = await _adminService.GetBookingReportAsync(startDate, endDate);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("reports/popular-cars")]
        public async Task<IActionResult> GetPopularCarsReport([FromQuery] int topCount = 10)
        {
            var result = await _adminService.GetPopularCarsReportAsync(topCount);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ============ SETTINGS ============

        [HttpGet("settings")]
        public async Task<IActionResult> GetAllSettings()
        {
            var result = await _adminService.GetAllSettingsAsync();

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("settings")]
        public async Task<IActionResult> UpdateSetting([FromBody] UpdateSystemSettingDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _adminService.UpdateSettingAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }

}
