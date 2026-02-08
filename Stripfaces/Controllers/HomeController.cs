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

            ViewBag.FeaturedVideos = featuredVideos;
            ViewBag.ModelsWithVideos = modelsWithVideos;

            return View();
        }
    }
}