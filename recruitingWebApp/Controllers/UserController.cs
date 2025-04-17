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


        public BlobServiceClient GetBlobServiceClient()
        {
            var connStr = _config["AzureStorage:ConnectionString"];
            return new BlobServiceClient(connStr);
        }




        /* TODO:
         * Convert uploaded image to bit array or send video to cdn
         * send post to db
         * update db too in console
         */
        [HttpPost]
        public async Task<IActionResult> AddPost(string Caption, IFormFile file)
        {

            var userId = HttpContext.Session.GetInt32("UserId");

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("File", "Please upload a file.");
                return RedirectToAction("UserProfile");
            }

            // Get BlobServiceClient using your method
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

        //TODO: add function that gets users posts

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

            return View(user);
        }
    }
}
