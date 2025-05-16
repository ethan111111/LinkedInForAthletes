using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PostgreSQL.Data;
using recruitingWebApp.Models;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using recruitingWebApp.Migrations;

namespace recruitingWebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UserController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        //blob storage for post media files
        public BlobServiceClient GetBlobServiceClient()
        {
            var connStr = _config["AzureStorage:ConnectionString"];
            return new BlobServiceClient(connStr);
        }




        //add post from the user profile
        [HttpPost]
        [RequestSizeLimit(200_000_000)]
        public async Task<IActionResult> AddPost(string Caption, IFormFile file)
        {

            var userId = HttpContext.Session.GetInt32("UserId");

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please upload a file.");
                return RedirectToAction("UserProfile");
            }

            // Get BlobServiceClient
            var blobServiceClient = GetBlobServiceClient();

            // Get container reference
            var containerClient = blobServiceClient.GetBlobContainerClient("userposts");
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Generate a unique name for the blob
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blobClient = containerClient.GetBlobClient(fileName);

            // Upload the file to Azure Blob Storage
            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            // Create post with Azure Blob URL
            var post = new Post
            {
                Caption = Caption,
                PostUrl = blobClient.Uri.ToString(), // save URL to DB
                Timestamp = DateTime.UtcNow, 
                ContentType = file.ContentType, // ex: "video/mp4"
                UserId = userId.Value
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserProfile");
        }


        //Deletes post from db so not seen by user anymore
        [HttpPost]
        public async Task<IActionResult> DeletePost(int postID)
        {
            var post = await _context.Posts.FindAsync(postID);
            if (post != null)
            {
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("UserProfile");
        }

        //Updates profile attributes
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string? FirstName, string? LastName, string? Username, string? Bio)
        {
            Console.WriteLine("UpdateProfile called");

            var userId = HttpContext.Session.GetInt32("UserId");
          

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null)
            {
                return NotFound(); // User not found in DB
            }

            // Only update fields if new values are provided
            if (!string.IsNullOrWhiteSpace(FirstName))
            {
                user.FirstName = FirstName;
            }

            if (!string.IsNullOrWhiteSpace(LastName))
            {
                user.LastName = LastName;
            }

            if (!string.IsNullOrWhiteSpace(Username))
            {
                user.Username = Username;
            }

            if (Bio != null) // Allow clearing bio
            {
                user.Bio = Bio;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserProfile");
        }

        //Public user profile view
        public async Task<IActionResult> ViewUserProfile(int id)
        {
            var user = await _context.Users
                .Include(u => u.ProfileImage)
                .Include(u => u.Posts)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var measurements = await _context.Measurment
                .Where(m => m.UserId == id)
                .ToListAsync();

            ViewBag.Measurements = measurements;

            return View("PublicUserProfileView", user);
        }

        //Add measuremnts to users profile 
        [HttpPost]
        public async Task<IActionResult> AddMeasurement(string Measurement, string Value)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null || string.IsNullOrWhiteSpace(Measurement) || string.IsNullOrWhiteSpace(Value))
            {
                return RedirectToAction("UserProfile");
            }

            var entry = new Measurments
            {
                UserId = userId.Value,
                Measurement = Measurement,
                Value = Value
            };

            _context.Measurment.Add(entry);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserProfile");
        }


        public async Task<IActionResult> UserProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                // Not logged in
                return RedirectToAction("Login", "Login");
            }

            // Load user including profile image and posts
            var user = await _context.Users
                .Include(u => u.ProfileImage)
                .Include(u => u.Posts)
                .FirstOrDefaultAsync(u => u.Id == userId.Value);

            if (user == null)
            {
                // Could not find user in DB
                return View(null);
            }

            var measurements = await _context.Measurment
            .Where(m => m.UserId == userId.Value)
            .ToListAsync();

            ViewBag.Measurements = measurements;


            return View(user);
        }
    }
}
