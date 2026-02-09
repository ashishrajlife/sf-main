using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace stripfaces.Services
{
    public class FileUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;

        public FileUploadService(IWebHostEnvironment env, IConfiguration config)
        {
            _env = env;
            _config = config;
        }

        public async Task<string> SaveVideo(IFormFile videoFile, string modelName)
        {
            // Sanitize model name (remove special characters)
            var safeModelName = System.Text.RegularExpressions.Regex.Replace(
                modelName.ToLower(),
                "[^a-z0-9]",
                ""
            );

            // Create model folder if not exists
            var modelFolder = Path.Combine("uploads", "videos", safeModelName);
            var fullPath = Path.Combine(_env.WebRootPath, modelFolder);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(videoFile.FileName)}";
            var filePath = Path.Combine(modelFolder, fileName);
            var fullFilePath = Path.Combine(_env.WebRootPath, filePath);

            // Save file
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await videoFile.CopyToAsync(stream);
            }

            // Return relative path for database
            return $"/{filePath.Replace("\\", "/")}";
        }

        // ADD THIS METHOD
        public async Task<string> SaveThumbnail(IFormFile thumbnailFile, string modelName)
        {
            // Sanitize model name
            var safeModelName = System.Text.RegularExpressions.Regex.Replace(
                modelName.ToLower(),
                "[^a-z0-9]",
                ""
            );

            // Create thumbnail folder if not exists
            var thumbnailFolder = Path.Combine("uploads", "thumbnails", safeModelName);
            var fullPath = Path.Combine(_env.WebRootPath, thumbnailFolder);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(thumbnailFile.FileName)}";
            var filePath = Path.Combine(thumbnailFolder, fileName);
            var fullFilePath = Path.Combine(_env.WebRootPath, filePath);

            // Save file
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await thumbnailFile.CopyToAsync(stream);
            }

            // Return relative path for database
            return $"/{filePath.Replace("\\", "/")}";
        }

        public async Task<string> SaveProfilePicture(IFormFile profilePicFile)
        {
            // Create profile picture folder if not exists
            var profilePicFolder = Path.Combine("uploads", "profilepicture");
            var fullPath = Path.Combine(_env.WebRootPath, profilePicFolder);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(profilePicFile.FileName)}";
            var filePath = Path.Combine(profilePicFolder, fileName);
            var fullFilePath = Path.Combine(_env.WebRootPath, filePath);

            // Save file
            using (var stream = new FileStream(fullFilePath, FileMode.Create))
            {
                await profilePicFile.CopyToAsync(stream);
            }

            // Return relative path for database
            return $"/{filePath.Replace("\\", "/")}";
        }

        public async Task<string> GenerateThumbnail(string videoPath, string modelName)
        {
            // For now, return a placeholder
            // You'll implement FFmpeg later

            var safeModelName = System.Text.RegularExpressions.Regex.Replace(
                modelName.ToLower(),
                "[^a-z0-9]",
                ""
            );

            var thumbnailFolder = Path.Combine("uploads", "thumbnails", safeModelName);
            var fullPath = Path.Combine(_env.WebRootPath, thumbnailFolder);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            // Create a default thumbnail (you can save a default image file)
            var fileName = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(thumbnailFolder, fileName);

            // For now, return a placeholder URL
            return "/images/default-thumbnail.jpg";
        }

        public bool DeleteVideo(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var fullPath = Path.Combine(_env.WebRootPath, filePath.TrimStart('/'));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return true;
            }
            return false;
        }
    }
}