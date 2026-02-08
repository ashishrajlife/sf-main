// Controllers/UserController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using stripfaces.Data;
using stripfaces.Models;
using stripfaces.ViewModels;

namespace stripfaces.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;


        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }
        // Check authentication
        private bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("UserId"));
        }

        private bool IsUser()
        {
            return HttpContext.Session.GetString("Role") == "user";
        }

        // GET: /User
        // GET: User/Videos
        public async Task<IActionResult> Index()
        {
            var models = await _context.Models
                .Where(m => m.IsActive)
                .ToListAsync();

            // Get video counts for each model
            var videoCounts = await _context.Videos
                .GroupBy(v => v.ModelId)
                .Select(g => new { ModelId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ModelId, x => x.Count);

            ViewBag.VideoCounts = videoCounts;

            return View(models);
        }

        // GET: /User/Profile
        public IActionResult Profile()
        {
            if (!IsAuthenticated() || !IsUser())
                return RedirectToAction("Login", "Auth");

            return View();
        }



        // GET: User/Videos/ModelVideos/{modelId}
        public async Task<IActionResult> ModelVideos(int modelId)
        {
            var model = await _context.Models.FindAsync(modelId);
            if (model == null) return NotFound();

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

            // Yeh line check karo:
            return View(viewModel); // ← Correct
                                    // return View("ModelVideos", viewModel); // ← Agar subfolder me hai
        }

        // GET: User/Videos/Watch/{id}
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


    }
}