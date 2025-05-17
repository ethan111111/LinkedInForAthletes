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

        public async Task<IActionResult> Index()
        {
            var currentUsername = User.Identity?.Name;

            var posts = await _context.Posts
                .Include(p => p.User)
                    .ThenInclude(u => u.ProfileImage)
                .Where(p => p.User.Username != currentUsername) // exclude current user
                .OrderByDescending(p => p.Timestamp)             // most recent first
                .ToListAsync();

            return View(posts); // Send list of posts to view
        }



     
        //changing profile pic (deletes old one from db and adds new one)
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
