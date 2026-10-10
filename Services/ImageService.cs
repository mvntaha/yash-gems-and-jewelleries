using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Yash_Gems___Jewelleries.Helpers;
using Yash_Gems___Jewelleries.Interfaces;

namespace Yash_Gems___Jewelleries.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<ImageService> _logger;

        public ImageService(IWebHostEnvironment webHostEnvironment, ILogger<ImageService> logger)
        {
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        public async Task<string?> SaveImageAsync(IFormFile? imageFile, string category = IImageService.ProductsMain)
        {
            try
            {
                // Basic check
                if (imageFile == null || imageFile.Length == 0) return null;

                // Use FileValidationHelper for comprehensive security validation
                if (!FileValidationHelper.ValidateFile(imageFile, out string errorMessage))
                {
                    _logger.LogWarning("Image validation failed for file {FileName}: {ErrorMessage}", imageFile.FileName, errorMessage);
                    return null;
                }

                // Generate unique and sanitized filename using GUID
                var fileName = FileValidationHelper.GenerateUniqueFileName(imageFile.FileName);
                
                // Define paths
                var relativeFolder = Path.Combine(IImageService.UploadsRoot, category).Replace("\\", "/");
                var absoluteFolder = Path.Combine(_webHostEnvironment.WebRootPath, relativeFolder);

                // Ensure directory exists
                if (!Directory.Exists(absoluteFolder))
                {
                    Directory.CreateDirectory(absoluteFolder);
                }

                var filePath = Path.Combine(absoluteFolder, fileName);

                // Save file securely
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Return normalized relative path (no leading slash for better flexibility)
                return $"{relativeFolder}/{fileName}".Replace("//", "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while saving image for category {Category}", category);
                return null;
            }
        }

        public void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            try
            {
                // Normalize path and combine with WebRoot
                var relativePath = imageUrl.TrimStart('/');
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Successfully deleted image: {ImagePath}", imageUrl);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete image: {ImagePath}", imageUrl);
            }
        }

        public bool IsValidImage(IFormFile? file)
        {
            return file != null && file.Length > 0 && FileValidationHelper.ValidateFile(file, out _);
        }
    }
}
