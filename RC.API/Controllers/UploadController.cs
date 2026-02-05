using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RC.Application.Interfaces.Services;

namespace RC.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;

        public UploadController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        /// <summary>
        /// Upload single image
        /// </summary>
        [HttpPost("image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string folder = "general")
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { Success = false, Message = "No file uploaded" });

                var imageUrl = await _fileUploadService.UploadImageAsync(file, folder);

                return Ok(new
                {
                    Success = true,
                    Message = "Image uploaded successfully",
                    Data = new { ImageUrl = imageUrl }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Upload multiple images
        /// </summary>
        [HttpPost("images")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImages(List<IFormFile> files, [FromQuery] string folder = "general")
        {
            try
            {
                if (files == null || files.Count == 0)
                    return BadRequest(new { Success = false, Message = "No files uploaded" });

                var imageUrls = await _fileUploadService.UploadImagesAsync(files, folder);

                return Ok(new
                {
                    Success = true,
                    Message = $"{imageUrls.Count} images uploaded successfully",
                    Data = new { ImageUrls = imageUrls }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        /// <summary>
        /// Delete image by URL
        /// </summary>
        [HttpDelete("image")]
        public async Task<IActionResult> DeleteImage([FromQuery] string imageUrl)
        {
            try
            {
                var deleted = await _fileUploadService.DeleteImageAsync(imageUrl);

                if (!deleted)
                    return NotFound(new { Success = false, Message = "Image not found" });

                return Ok(new
                {
                    Success = true,
                    Message = "Image deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}