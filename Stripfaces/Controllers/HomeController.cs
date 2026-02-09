    // Controllers/HomeController.cs
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using stripfaces.Data;
    using stripfaces.Models;

    namespace stripfaces.Controllers
    {
        public class HomeController : Controller
        {
            private readonly ApplicationDbContext _context;

            public HomeController(ApplicationDbContext context)
            {
                _context = context;
            }

        public async Task<IActionResult> Index()
        {
            // Get all active models with their videos
            var modelsWithVideos = await _context.Models
                .Where(m => m.IsActive)
                .Include(m => m.Videos.Where(v => v.IsApproved))
                .OrderBy(m => m.Name)
                .ToListAsync();

            // Get featured videos
            var featuredVideos = await _context.Videos
                .Where(v => v.IsApproved && v.IsFeatured)
                .Include(v => v.Model)
                .OrderByDescending(v => v.UploadedAt)
                .Take(6)
                .ToListAsync();

            // GET TOP 10 RECENT VIDEOS (for all users - logged in or not)
            var recentVideos = await _context.Videos
                .Where(v => v.IsApproved) // Only show approved videos
                .Include(v => v.Model)
                .OrderByDescending(v => v.UploadedAt) // Sort by newest first
                .Take(10) // Take only top 10
                .Select(v => new
                {
                    v.VideoId,
                    v.Title,
                    v.Description,
                    v.FilePath,
                    Thumbnail = v.ThumbnailPath ?? "/images/default-thumbnail.jpg",
                    ModelName = v.Model.Name,
                    v.Views,
                    v.UploadedAt,
                    Duration = v.Duration.HasValue ?
                        TimeSpan.FromSeconds(v.Duration.Value).ToString(@"mm\:ss") : "00:00",
                    v.IsFeatured
                })
                .ToListAsync();

            ViewBag.FeaturedVideos = featuredVideos;
            ViewBag.ModelsWithVideos = modelsWithVideos;
            ViewBag.RecentVideos = recentVideos; // Add recent videos to ViewBag

            // Check if user is logged in
            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                ViewData["IsLoggedIn"] = true;
                ViewData["Username"] = HttpContext.Session.GetString("Username");
                ViewData["Role"] = HttpContext.Session.GetString("Role");
            }
            else
            {
                ViewData["IsLoggedIn"] = false;
            }

            return View();
        }
        
      }
    }