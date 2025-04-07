using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using recruitingWebApp.Models;
using PostgreSQL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace recruitingWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        /*
        [HttpPost]
        public async Task<IActionResult> Upload(string FirstName, string LastName, string Bio, string Username,string Password, IFormFile file)
        {
            if (file == null || file.Length == 0 || string.IsNullOrEmpty(FirstName) || string.IsNullOrEmpty(LastName) || string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ViewData["Message"] = "Please provide all required fields.";
                return View("Index");
            }

            // Remove existing user and profile picture before adding a new one
            var existingUsers = _context.Users.ToList();
            if (existingUsers.Any())
            {
                _context.Users.RemoveRange(existingUsers);
                await _context.SaveChangesAsync();
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
            return View("Index");
        }
        */

        [HttpPost]
        public async Task<IActionResult> UpdateProfilePic(int UserId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ViewData["Message"] = "Please select an image.";
                return RedirectToAction("DisplayImages");
            }

            var user = _context.Users.Include(u => u.ProfileImage).FirstOrDefault(u => u.Id == UserId);
            if (user == null)
            {
                return NotFound();
            }

            // Delete the old profile picture if it exists
            if (user.ProfileImage != null)
            {
                _context.Images.Remove(user.ProfileImage);
                await _context.SaveChangesAsync();
            }

            // Save the new profile picture
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                var newProfilePic = new ProfilePic
                {
                    ImageData = memoryStream.ToArray()
                };
                _context.Images.Add(newProfilePic);
                await _context.SaveChangesAsync();

                // Update user's profile picture reference
                user.ProfilePicId = newProfilePic.Id;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("UserProfile", "User");
        }


        public IActionResult UserProfile()
        {
            var users = _context.Users
                .Include(u => u.ProfileImage) // Ensure profile pictures are loaded
                .ToList();

            return View(users);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
