using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using stripfaces.Data;
using stripfaces.Models;
using stripfaces.Services;
using System.Linq;
using System.Threading.Tasks;

namespace stripfaces.Controllers
{
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FileUploadService _fileUploadService;
        public AdminController(ApplicationDbContext context, FileUploadService fileUploadService)
        {
            _context = context;
            _fileUploadService = fileUploadService;
        }

        // Check authentication
        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
        }

        private bool IsAdmin()
        {
            return HttpContext.Session.GetString("Role") == "admin";
        }

        // GET: /Admin
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated() || !IsAdmin())
                return RedirectToAction("Login", "Auth");

            ViewData["Username"] = HttpContext.Session.GetString("Username");

            // Get statistics for dashboard
            var totalUsers = await _context.Users.CountAsync();
            var adminCount = await _context.Users.CountAsync(u => u.Role == "admin");
            var totalVideos = await _context.Videos.CountAsync();
            var totalViews = await _context.Videos.SumAsync(v => v.Views);
            var totalModels = await _context.Models.CountAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.AdminCount = adminCount;
            ViewBag.TotalVideos = totalVideos;
            ViewBag.TotalViews = totalViews;
            ViewBag.TotalModels = totalModels;

            var antiforgery = HttpContext.RequestServices.GetService<IAntiforgery>();
            var tokenSet = antiforgery.GetAndStoreTokens(HttpContext);
            ViewBag.AntiforgeryToken = tokenSet.RequestToken;

            return View();
        }

        // GET: /Admin/Users
        [HttpGet]
        [Route("Users")]
        public async Task<IActionResult> Users()
        {
            if (!IsAuthenticated() || !IsAdmin())
                return RedirectToAction("Login", "Auth");

            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        //// GET: /Admin/GetVideos (AJAX endpoint for videos list)
        //[HttpGet]
        //[Route("GetVideos")]
        //public async Task<IActionResult> GetVideos()
        //{
        //    if (!IsAuthenticated() || !IsAdmin())
        //        return Json(new { success = false, message = "Unauthorized" });

        //    var videos = await _context.Videos
        //        .Include(v => v.Model)
        //        .Include(v => v.UploadedBy)
        //        .OrderByDescending(v => v.UploadedAt)
        //        .Select(v => new
        //        {
        //            v.VideoId,
        //            v.Title,
        //            v.FilePath,
        //            Thumbnail = v.ThumbnailPath ?? "/images/default-thumbnail.jpg",
        //            ModelName = v.Model.Name,
        //            ModelId = v.Model.ModelId,
        //            Views = v.Views,
        //            UploadedAt = v.UploadedAt.ToString("MMM dd, yyyy HH:mm"),
        //            IsApproved = v.IsApproved,
        //            IsFeatured = v.IsFeatured,
        //            Duration = v.Duration.HasValue ?
        //                TimeSpan.FromSeconds(v.Duration.Value).ToString(@"mm\:ss") : "00:00"
        //        })
        //        .ToListAsync();

        //    return Json(new { success = true, data = videos });
        //}

        //// DELETE: /Admin/DeleteVideo/{id} (AJAX)
        //[HttpPost]
        //[Route("DeleteVideo/{id}")]
        //public async Task<IActionResult> DeleteVideo(int id)
        //{
        //    if (!IsAuthenticated() || !IsAdmin())
        //        return Json(new { success = false, message = "Unauthorized" });

        //    try
        //    {
        //        var video = await _context.Videos.FindAsync(id);
        //        if (video == null)
        //            return Json(new { success = false, message = "Video not found" });

        //        // Delete physical file (optional)
        //        // _fileUploadService.DeleteVideo(video.FilePath);

        //        _context.Videos.Remove(video);
        //        await _context.SaveChangesAsync();

        //        return Json(new { success = true, message = "Video deleted successfully" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}

        //// GET: /Admin/GetModels (AJAX - for dropdown)
        //[HttpGet]
        //[Route("GetModels")]
        //public async Task<IActionResult> GetModels()
        //{
        //    if (!IsAuthenticated() || !IsAdmin())
        //        return Json(new { success = false, message = "Unauthorized" });

        //    var models = await _context.Models
        //        .Where(m => m.IsActive)
        //        .Select(m => new { m.ModelId, m.Name })
        //        .ToListAsync();

        //    return Json(new { success = true, data = models });
        //}

        [HttpPost]
        [Route("UploadVideo")]
        [RequestSizeLimit(2147483648)] // 2GB limit
        public async Task<IActionResult> UploadVideo([FromForm] VideoUploadViewModel model)
        {
            if (!IsAuthenticated() || !IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                // Clear ModelState for Models property since we don't want to bind it
                ModelState.Remove("Models");

                if (ModelState.IsValid)
                {
                    // Get model
                    var selectedModel = await _context.Models.FindAsync(model.ModelId);
                    if (selectedModel == null)
                        return Json(new { success = false, message = "Model not found" });

                    // Save video file
                    var videoPath = await _fileUploadService.SaveVideo(model.VideoFile, selectedModel.Name);

                    // Save thumbnail if provided, otherwise generate default
                    string thumbnailPath;
                    if (model.ThumbnailFile != null && model.ThumbnailFile.Length > 0)
                    {
                        thumbnailPath = await _fileUploadService.SaveThumbnail(model.ThumbnailFile, selectedModel.Name);
                    }
                    else
                    {
                        thumbnailPath = await _fileUploadService.GenerateThumbnail(videoPath, selectedModel.Name);
                    }

                    // Get user ID from session
                    var userIdStr = HttpContext.Session.GetString("UserId");
                    if (!int.TryParse(userIdStr, out int userId))
                    {
                        return Json(new { success = false, message = "User session invalid" });
                    }

                    // Create video record
                    var video = new Video
                    {
                        Title = model.Title,
                        Description = model.Description,
                        FilePath = videoPath,
                        ThumbnailPath = thumbnailPath,
                        ModelId = model.ModelId,
                        UploadedById = userId,
                        Tags = model.Tags,
                        IsFeatured = model.IsFeatured,
                        FileSize = model.VideoFile.Length,
                        IsApproved = true, // Auto-approve for admin
                        UploadedAt = DateTime.Now
                    };

                    _context.Videos.Add(video);
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        message = "Video uploaded successfully",
                        videoId = video.VideoId
                    });
                }

                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = string.Join(", ", errors) });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Upload error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return Json(new
                {
                    success = false,
                    message = $"Upload failed: {ex.Message}"
                });
            }
        }

        // GET: /Admin/GetVideos (FIXED property names)
        [HttpGet]
        [Route("GetVideos")]
        public async Task<IActionResult> GetVideos()
        {
            if (!IsAuthenticated() || !IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var videos = await _context.Videos
                    .Include(v => v.Model)
                    .Include(v => v.UploadedBy)
                    .OrderByDescending(v => v.UploadedAt)
                    .Select(v => new
                    {
                        videoId = v.VideoId, // lowercase for JavaScript
                        title = v.Title,
                        description = v.Description,
                        filePath = v.FilePath,
                        thumbnail = v.ThumbnailPath ?? "/images/default-thumbnail.jpg",
                        thumbnailPath = v.ThumbnailPath ?? "/images/default-thumbnail.jpg",
                        model = v.Model != null ? new
                        {
                            modelId = v.Model.ModelId,
                            name = v.Model.Name
                        } : null,
                        modelName = v.Model != null ? v.Model.Name : "Unknown",
                        views = v.Views,
                        uploadedAt = v.UploadedAt.ToString("MMM dd, yyyy HH:mm"),
                        uploadedDate = v.UploadedAt.ToString("MMM dd, yyyy"),
                        uploadedTime = v.UploadedAt.ToString("HH:mm"),
                        isApproved = v.IsApproved,
                        isFeatured = v.IsFeatured,
                        duration = v.Duration,
                        durationFormatted = v.Duration.HasValue ?
                            TimeSpan.FromSeconds(v.Duration.Value).ToString(@"mm\:ss") : "00:00",
                        tags = v.Tags
                    })
                    .ToListAsync();

                return Json(new { success = true, data = videos });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // DELETE: /Admin/DeleteVideo/{id} (FIXED - with file deletion)
        [HttpPost]
        [Route("DeleteVideo/{id}")]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            if (!IsAuthenticated() || !IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var video = await _context.Videos.FindAsync(id);
                if (video == null)
                    return Json(new { success = false, message = "Video not found" });

                // Delete physical files
                if (!string.IsNullOrEmpty(video.FilePath))
                {
                    _fileUploadService.DeleteVideo(video.FilePath);
                }

                // Delete thumbnail if not default
                if (!string.IsNullOrEmpty(video.ThumbnailPath) &&
                    !video.ThumbnailPath.Contains("/images/default-thumbnail.jpg"))
                {
                    var thumbnailPath = video.ThumbnailPath;
                    if (thumbnailPath.StartsWith("/"))
                        thumbnailPath = thumbnailPath.Substring(1);

                    var fullThumbnailPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", thumbnailPath);
                    if (System.IO.File.Exists(fullThumbnailPath))
                    {
                        System.IO.File.Delete(fullThumbnailPath);
                    }
                }

                _context.Videos.Remove(video);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Video deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: /Admin/GetModels (FIXED property names)
        [HttpGet]
        [Route("GetModels")]
        public async Task<IActionResult> GetModels()
        {
            if (!IsAuthenticated() || !IsAdmin())
                return Json(new { success = false, message = "Unauthorized" });

            try
            {
                var models = await _context.Models
                    .Where(m => m.IsActive)
                    .Select(m => new {
                        ModelId = m.ModelId, // Capital for JavaScript
                        Name = m.Name
                    })
                    .ToListAsync();

                return Json(new { success = true, data = models });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


    }
}