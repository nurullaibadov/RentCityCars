using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RC.Application.DTOs.Car;
using RC.Application.Interfaces.Services;

namespace RC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarService _carService;

        public CarsController(ICarService carService)
        {
            _carService = carService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CarSearchRequestDto request)
        {
            var result = await _carService.GetAllCarsAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _carService.GetCarByIdAsync(id);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetFeatured([FromQuery] int count = 10)
        {
            var result = await _carService.GetFeaturedCarsAsync(count);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate >= endDate)
                return BadRequest("Start date must be before end date");

            var result = await _carService.GetAvailableCarsAsync(startDate, endDate);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("check-availability")]
        public async Task<IActionResult> CheckAvailability([FromQuery] Guid carId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var result = await _carService.CheckAvailabilityAsync(carId, startDate, endDate);

            return Ok(result);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCarRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _carService.CreateCarAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCarRequestDto request)
        {
            if (id != request.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _carService.UpdateCarAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _carService.DeleteCarAsync(id);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }

}
