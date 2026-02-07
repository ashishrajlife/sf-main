// Controllers/AdminController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using stripfaces.Data;
using stripfaces.Data;
using stripfaces.Models;

namespace stripfaces.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
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
        public IActionResult Index()
        {
            if (!IsAuthenticated() || !IsAdmin())
                return RedirectToAction("Login", "Auth");

            ViewData["Username"] = HttpContext.Session.GetString("Username");

            return View();
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            if (!IsAuthenticated() || !IsAdmin())
                return RedirectToAction("Login", "Auth");

            var users = await _context.Users.ToListAsync();
            return View(users);
        }
    }
}