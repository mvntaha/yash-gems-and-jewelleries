using Microsoft.AspNetCore.Http;

namespace Yash_Gems___Jewelleries.Services
{
    public interface IImageService
    {
        // Centralized Folder Constants
        public const string UploadsRoot = "uploads";
        public const string Categories = "categories";
        public const string Brands = "brands";
        public const string ProductsMain = "products/main";
        public const string ProductsGallery = "products/gallery";
        public const string Banners = "banners";
        public const string Profile = "profile";
        public const string Certifications = "certifications";

        /// <summary>
        /// Saves an image file to the specified category folder and returns the normalized relative path.
        /// Normalized path example: "uploads/products/main/example.jpg"
        /// </summary>
        Task<string?> SaveImageAsync(IFormFile? imageFile, string category = ProductsMain);

        /// <summary>
        /// Deletes an image file given its relative path.
        /// </summary>
        void DeleteImage(string? imageUrl);

        /// <summary>
        /// Validates if the file is a valid image (extension, size, and signature).
        /// </summary>
        bool IsValidImage(IFormFile? file);
    }
}
