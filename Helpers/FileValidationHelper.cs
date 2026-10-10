using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Yash_Gems___Jewelleries.Helpers
{
    /// <summary>
    /// Provides comprehensive file upload security validation
    /// </summary>
    public static class FileValidationHelper
    {
        // Maximum file size: 5MB
        private const long MaxFileSize = 5 * 1024 * 1024;

        // Allowed image extensions
        private static readonly HashSet<string> AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        // File signatures (magic bytes) for validation
        // This prevents users from renaming .txt to .jpg
        private static readonly Dictionary<string, List<byte[]>> FileSignatures = new Dictionary<string, List<byte[]>>
        {
            {
                ".jpg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, // JPEG JFIF
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 }, // JPEG EXIF
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 }, // JPEG
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }, // JPEG
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }  // JPEG
                }
            },
            {
                ".jpeg", new List<byte[]>
                {
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 },
                    new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }
                }
            },
            {
                ".png", new List<byte[]>
                {
                    new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
                }
            },
            {
                ".webp", new List<byte[]>
                {
                    new byte[] { 0x52, 0x49, 0x46, 0x46 } // RIFF (WebP starts with RIFF)
                }
            }
        };

        /// <summary>
        /// Validates uploaded file for security
        /// </summary>
        /// <param name="file">The uploaded file</param>
        /// <param name="errorMessage">Output error message if validation fails</param>
        /// <returns>True if file is valid, false otherwise</returns>
        public static bool ValidateFile(IFormFile? file, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Check if file exists
            if (file == null || file.Length == 0)
            {
                errorMessage = "No file was uploaded.";
                return false;
            }

            // Check file size
            if (file.Length > MaxFileSize)
            {
                errorMessage = $"File size exceeds the maximum allowed size of {MaxFileSize / (1024 * 1024)}MB.";
                return false;
            }

            // Get file extension
            var fileName = file.FileName ?? "unknown";
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            // Check if extension is allowed
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                errorMessage = $"File type '{extension}' is not allowed. Allowed types: {string.Join(", ", AllowedExtensions)}";
                return false;
            }

            // Validate file signature (magic bytes)
            if (!ValidateFileSignature(file, extension))
            {
                errorMessage = "File content does not match its extension. Possible file type mismatch or corrupted file.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates file signature (magic bytes) to prevent fake extensions
        /// </summary>
        private static bool ValidateFileSignature(IFormFile file, string extension)
        {
            if (!FileSignatures.ContainsKey(extension))
            {
                return true; // If we don't have signature for this extension, skip validation
            }

            var signatures = FileSignatures[extension];

            using (var reader = new BinaryReader(file.OpenReadStream()))
            {
                // Read the first 8 bytes (longest signature we check)
                var headerBytes = reader.ReadBytes(8);

                // Check if file header matches any of the valid signatures
                return signatures.Any(signature =>
                {
                    if (headerBytes.Length < signature.Length)
                        return false;

                    return signature.SequenceEqual(headerBytes.Take(signature.Length));
                });
            }
        }

        /// <summary>
        /// Sanitizes filename to prevent path traversal attacks
        /// </summary>
        /// <param name="filename">Original filename</param>
        /// <returns>Sanitized filename</returns>
        public static string SanitizeFileName(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
            {
                return $"file_{Guid.NewGuid():N}";
            }

            // Remove path information
            filename = Path.GetFileName(filename);

            // Remove invalid characters
            var invalidChars = Path.GetInvalidFileNameChars();
            filename = string.Join("_", filename.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            // Remove potentially dangerous sequences
            filename = filename.Replace("..", "_");
            filename = filename.Replace("./", "_");
            filename = filename.Replace("../", "_");
            filename = filename.Replace("\\", "_");

            // Limit filename length (keep extension)
            var extension = Path.GetExtension(filename);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(filename);

            if (nameWithoutExtension.Length > 50)
            {
                nameWithoutExtension = nameWithoutExtension.Substring(0, 50);
            }

            // Ensure filename is not empty after sanitization
            if (string.IsNullOrWhiteSpace(nameWithoutExtension))
            {
                nameWithoutExtension = $"file_{Guid.NewGuid():N}";
            }

            return $"{nameWithoutExtension}{extension}";
        }

        /// <summary>
        /// Generates a unique filename to prevent collisions
        /// </summary>
        /// <param name="originalFilename">Original filename</param>
        /// <returns>Unique filename</returns>
        public static string GenerateUniqueFileName(string originalFilename)
        {
            var sanitizedName = SanitizeFileName(originalFilename);
            var extension = Path.GetExtension(sanitizedName);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var guid = Guid.NewGuid().ToString("N").Substring(0, 8);

            return $"{Path.GetFileNameWithoutExtension(sanitizedName)}_{timestamp}_{guid}{extension}";
        }
    }
}
