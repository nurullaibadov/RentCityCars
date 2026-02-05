using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RC.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Infrastructure.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IConfiguration _configuration;
        private readonly string _uploadPath;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        public FileUploadService(IConfiguration configuration)
        {
            _configuration = configuration;

            // For local development, save to wwwroot/uploads
            // In production, you would use Cloudinary or Azure Blob Storage
            _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder = "cars")
        {
            try
            {
                // Validate file
                if (file == null || file.Length == 0)
                    throw new Exception("File is empty");

                if (file.Length > _maxFileSize)
                    throw new Exception($"File size exceeds maximum allowed size of {_maxFileSize / 1024 / 1024}MB");

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                    throw new Exception($"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var folderPath = Path.Combine(_uploadPath, folder);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return URL
                var baseUrl = _configuration["App:BackendUrl"] ?? "https://localhost:7001";
                return $"{baseUrl}/uploads/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading file: {ex.Message}");
            }
        }

        public async Task<List<string>> UploadImagesAsync(List<IFormFile> files, string folder = "cars")
        {
            var uploadedUrls = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var url = await UploadImageAsync(file, folder);
                    uploadedUrls.Add(url);
                }
                catch
                {
                    // Continue with other files if one fails
                    continue;
                }
            }

            return uploadedUrls;
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                await Task.CompletedTask;

                // Extract filename from URL
                var uri = new Uri(imageUrl);
                var relativePath = uri.AbsolutePath.TrimStart('/');
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteImagesAsync(List<string> imageUrls)
        {
            var allDeleted = true;

            foreach (var url in imageUrls)
            {
                var deleted = await DeleteImageAsync(url);
                if (!deleted)
                    allDeleted = false;
            }

            return allDeleted;
        }
    }

}