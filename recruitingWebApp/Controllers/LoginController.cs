using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostgreSQL.Data;
using recruitingWebApp.Models;

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
        //TODO: make it so bio isnt needed to create a profile
        //TODO: make it so that if there isnt a profile pic uploaded give user a default profilepic
        [HttpPost]
        public async Task<IActionResult> Upload(string FirstName, string LastName, string Bio, string Username, string Password, IFormFile file)
        {
            if (file == null || file.Length == 0 || string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ViewData["Message"] = "Please provide all required fields.";
                return View("CreateProfile");
            }

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var profilePic = new ProfilePic
                {
                    ImageData = memoryStream.ToArray()
                };
                _context.Images.Add(profilePic);
                await _context.SaveChangesAsync();

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
            }

            ViewData["Message"] = "User and profile picture uploaded successfully!";
            return View("Login");
        }

        [HttpPost]
        public async Task<IActionResult> UserLogin(string Username, string Password)
        {
            Console.WriteLine(" UserLogin called");

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

            // Save user ID in session
            HttpContext.Session.SetInt32("UserId", user.Id);

            //TODO: Remove this :
            //Show user's bio in alert
            TempData["AlertMessage"] = user.Bio ?? "No bio found.";

            return RedirectToAction("UserProfile", "User");
        }


        [HttpPost]
        public IActionResult Logout()
        {
            // Clear all session data (logs out the user)
            HttpContext.Session.Clear();

            // TODO: Remove
            TempData["AlertMessage"] = "You have been logged out.";

            // Redirect to the login page
            return RedirectToAction("Login", "Login");
        }
    }
}
