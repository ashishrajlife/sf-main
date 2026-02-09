using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using stripfaces.Data;
using stripfaces.Models;
using stripfaces.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace stripfaces.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /User
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Auth");

            ViewData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["Email"] = HttpContext.Session.GetString("Email");

            // Get user's watch history (if implemented later)
            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var watchHistory = await _context.Videos
                .Where(v => v.Views > 0) // Temporary - replace with actual watch history
                .Include(v => v.Model)
                .OrderByDescending(v => v.UploadedAt)
                .Take(6)
                .ToListAsync();

            // Get recommended videos
            var recommendedVideos = await _context.Videos
                .Where(v => v.IsApproved && v.IsFeatured)
                .Include(v => v.Model)
                .OrderByDescending(v => v.UploadedAt)
                .Take(8)
                .ToListAsync();

            // Get all active models
            var models = await _context.Models
                .Where(m => m.IsActive)
                .Include(m => m.Videos.Where(v => v.IsApproved))
                .OrderBy(m => m.Name)
                .ToListAsync();

            ViewBag.WatchHistory = watchHistory;
            ViewBag.RecommendedVideos = recommendedVideos;
            ViewBag.Models = models;

            return View();
        }

        // GET: /User/Profile
        public IActionResult Profile()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Auth");

            var userId = int.Parse(HttpContext.Session.GetString("UserId"));
            var user = _context.Users.Find(userId);

            return View(user);
        }

        // GET: /User/Videos
        public async Task<IActionResult> Videos()
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Auth");

            var models = await _context.Models
                .Where(m => m.IsActive)
                .ToListAsync();

            var videoCounts = await _context.Videos
                .Where(v => v.IsApproved)
                .GroupBy(v => v.ModelId)
                .Select(g => new { ModelId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ModelId, x => x.Count);

            ViewBag.VideoCounts = videoCounts;

            return View(models);
        }

        // GET: /User/Videos/ModelVideos/{modelId}
        public async Task<IActionResult> ModelVideos(int modelId)
        {
            if (!IsAuthenticated())
                return RedirectToAction("Login", "Auth");

            var model = await _context.Models
                .Include(m => m.Videos)
                .FirstOrDefaultAsync(m => m.ModelId == modelId);

            if (model == null)
            {
                return NotFound();
            }

            var videos = await _context.Videos
                .Where(v => v.ModelId == modelId && v.IsApproved)
                .Include(v => v.Model)
                .OrderByDescending(v => v.UploadedAt)
                .ToListAsync();

            var viewModel = new ModelVideosViewModel
            {
                Model = model,
                Videos = videos
            };

            return View(viewModel);
        }

        // GET: /User/Watch/{id}
        public async Task<IActionResult> Watch(int id)
        {
            var video = await _context.Videos
                .Include(v => v.Model)
                .FirstOrDefaultAsync(v => v.VideoId == id);

            if (video == null)
            {
                return NotFound();
            }

            // Increment view count
            video.Views++;
            await _context.SaveChangesAsync();

            return View(video);
        }

        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
        }
    }
}