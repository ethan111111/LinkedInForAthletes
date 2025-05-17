using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostgreSQL.Data;
using recruitingWebApp.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace recruitingWebApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public LoginController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult CreateProfile()
        {
            return View();
        }

        // Create user and upload profile picture
        [HttpPost]
        public async Task<IActionResult> Upload(string FirstName, string LastName, string Bio, string Username, string Password, IFormFile file)
        {
            if (string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ViewData["Message"] = "Please provide all required fields.";
                return View("CreateProfile");
            }

            var profilePic = new ProfilePic();

            if (file != null && file.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    profilePic.ImageData = memoryStream.ToArray();
                    _context.Images.Add(profilePic);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                // Create a default profile pic entry if none uploaded
                profilePic = new ProfilePic { ImageData = new byte[0] };
                _context.Images.Add(profilePic);
                await _context.SaveChangesAsync();
            }

            var user = new User
            {
                FirstName = FirstName,
                LastName = LastName,
                Username = Username,
                Password = Password,
                Bio = Bio,
                ProfilePicId = profilePic.Id
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            ViewData["Message"] = "User and profile picture uploaded successfully!";
            return View("Login");
        }

        // Login
        [HttpPost]
        public async Task<IActionResult> UserLogin(string Username, string Password)
        {
            Console.WriteLine("UserLogin called");

            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                TempData["AlertMessage"] = "Please provide all required fields.";
                return RedirectToAction("Login");
            }

            var user = await _context.Users
                .Include(u => u.ProfileImage)
                .FirstOrDefaultAsync(u => u.Username == Username && u.Password == Password);

            if (user == null)
            {
                TempData["AlertMessage"] = "Invalid username or password.";
                return RedirectToAction("Login");
            }

            // Store user ID in session
            HttpContext.Session.SetInt32("UserId", user.Id);

          
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            TempData["AlertMessage"] = user.Bio ?? "No bio found.";
            return RedirectToAction("UserProfile", "User");
        }

        // Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["AlertMessage"] = "You have been logged out.";
            return RedirectToAction("Login", "Login");
        }
    }
}
