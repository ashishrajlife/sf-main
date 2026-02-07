// Controllers/UserController.cs
using Microsoft.AspNetCore.Mvc;
using stripfaces.Models;

namespace stripfaces.Controllers
{
    public class UserController : Controller
    {
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
        public IActionResult Index()
        {
            if (!IsAuthenticated() || !IsUser())
                return RedirectToAction("Login", "Auth");

            ViewData["Username"] = HttpContext.Session.GetString("Username");
            ViewData["Email"] = HttpContext.Session.GetString("Email");

            return View();
        }

        // GET: /User/Profile
        public IActionResult Profile()
        {
            if (!IsAuthenticated() || !IsUser())
                return RedirectToAction("Login", "Auth");

            return View();
        }
    }
}