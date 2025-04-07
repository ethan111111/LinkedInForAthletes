using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using PostgreSQL.Data;
using recruitingWebApp.Models;

namespace recruitingWebApp.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }





        /* TODO:
         * Convert uploaded image to bit array or send video to cdn
         * send post to db
         * update db too in console
         */
        [HttpPost]
        public IActionResult AddPost(string? Caption, IFormFile file)
        {
            //get media type here and either send video or photo to cdn

            var post = new Post
            {
                Caption = Caption

            };

            _context.Posts.Add(post);



            // TODO: Save the post to DB or handle it
            // TODO: Redirect or return a view
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

            // Load user including profile image
            var user = await _context.Users
                .Include(u => u.ProfileImage)
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
