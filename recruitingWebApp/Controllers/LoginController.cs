using Microsoft.AspNetCore.Mvc;
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
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ViewData["Message"] = "Please provide all required fields.";
                return View("Login");
            }


            /*
                check username and password
                change authorization status
                call UserProfile (still have to write that function)
               
             */
            return RedirectToAction("Index", "HomeController");
        }
    }

}
