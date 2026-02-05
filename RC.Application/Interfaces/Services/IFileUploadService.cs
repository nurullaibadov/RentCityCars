using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

namespace RC.Application.Interfaces.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder = "cars");
        Task<List<string>> UploadImagesAsync(List<IFormFile> files, string folder = "cars");
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<bool> DeleteImagesAsync(List<string> imageUrls);
    }
}
