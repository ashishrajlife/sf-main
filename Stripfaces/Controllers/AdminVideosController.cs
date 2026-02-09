using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using stripfaces.Data;
using stripfaces.Models;
using stripfaces.Services;
using Stripfaces.Models;

namespace Stripfaces.Controllers
{
    public class AdminVideosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FileUploadService _uploadService;

        public AdminVideosController(ApplicationDbContext context, FileUploadService uploadService)
        {
            _context = context;
            _uploadService = uploadService;
        }

        // GET: Admin/Videos/Upload
        public IActionResult Upload()
        {
            var viewModel = new VideoUploadViewModel
            {
                Models = _context.Models
                    .Where(m => m.IsActive)
                    .Select(m => new SelectListItem
                    {
                        Value = m.ModelId.ToString(),
                        Text = m.Name
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        // POST: Admin/Videos/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(VideoUploadViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Get model name for folder
                var selectedModel = await _context.Models.FindAsync(model.ModelId);

                // Save video file
                var videoPath = await _uploadService.SaveVideo(
                    model.VideoFile,
                    selectedModel.Name
                );

                // Generate thumbnail
                string thumbnailPath;
                if (model.ThumbnailFile != null)
                {
                    thumbnailPath = await _uploadService.SaveThumbnail(
                        model.ThumbnailFile,
                        selectedModel.Name
                    );
                }
                else
                {
                    thumbnailPath = await _uploadService.GenerateThumbnail(
                        videoPath,
                        selectedModel.Name
                    );
                }

                // Create video record
                var video = new Video
                {
                    Title = model.Title,
                    Description = model.Description,
                    FilePath = videoPath,
                    ThumbnailPath = thumbnailPath,
                    ModelId = model.ModelId,
                    UploadedById = int.Parse(HttpContext.Session.GetString("UserId")),
                    Tags = model.Tags,
                    IsFeatured = model.IsFeatured,
                    FileSize = model.VideoFile.Length,
                    // Duration will be set later with FFmpeg
                };

                _context.Videos.Add(video);
                await _context.SaveChangesAsync();

                return RedirectToAction("Manage");
            }

            // If we got here, something failed
            model.Models = _context.Models
                .Where(m => m.IsActive)
                .Select(m => new SelectListItem
                {
                    Value = m.ModelId.ToString(),
                    Text = m.Name
                })
                .ToList();

            return View(model);
        }

        // GET: Admin/Videos/Manage
        public async Task<IActionResult> Manage()
        {
            var videos = await _context.Videos
                .Include(v => v.Model)
                .Include(v => v.UploadedBy)
                .OrderByDescending(v => v.UploadedAt)
                .ToListAsync();

            return View(videos);
        }

        // POST: Admin/Videos/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var video = await _context.Videos.FindAsync(id);

            if (video != null)
            {
                // Delete physical file
                _uploadService.DeleteVideo(video.FilePath);

                // Delete database record
                _context.Videos.Remove(video);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Video deleted successfully";
            }

            return RedirectToAction("Manage");
        }

        // Add these to your AdminVideosController.cs

        [HttpGet("GetModels")]
        public async Task<IActionResult> GetModels()
        {
            try
            {
                var models = await _context.Models
                    .Where(m => m.IsActive)
                    .Select(m => new
                    {
                        ModelId = m.ModelId,
                        Name = m.Name
                    })
                    .ToListAsync();

                return Json(models);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("GetVideos")]
        public async Task<IActionResult> GetVideos()
        {
            try
            {
                var videos = await _context.Videos
                    .Include(v => v.Model)
                    .OrderByDescending(v => v.UploadedAt)
                    .Select(v => new
                    {
                        VideoId = v.VideoId,
                        Title = v.Title,
                        Description = v.Description,
                        FilePath = v.FilePath,
                        ThumbnailPath = v.ThumbnailPath,
                        Duration = v.Duration,
                        Views = v.Views,
                        Model = v.Model != null ? new { ModelId = v.Model.ModelId, Name = v.Model.Name } : null,
                        UploadedAt = v.UploadedAt,
                        Tags = v.Tags,
                        IsFeatured = v.IsFeatured
                    })
                    .ToListAsync();

                return Json(videos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

    }
}
