using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RC.Application.DTOs.Booking;
using RC.Application.Interfaces.Services;
using System.Security.Claims;

namespace RC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBookingRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _bookingService.CreateBookingAsync(userId, request);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _bookingService.GetMyBookingsAsync(userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _bookingService.GetBookingByIdAsync(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("track/{bookingNumber}")]
        public async Task<IActionResult> TrackBooking(string bookingNumber)
        {
            var result = await _bookingService.GetBookingByNumberAsync(bookingNumber);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] CreateBookingRequestDto request)
        {
            var result = await _bookingService.CalculateBookingCostAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _bookingService.CancelBookingAsync(id, userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateBookingStatusRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _bookingService.UpdateBookingStatusAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }

}
