using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Test4712.Models;
using Test4712.Services;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using System.Net;
using Newtonsoft.Json;

namespace Test4712.Controllers
{
    public class AccountController : Controller
    {


        private readonly HttpClient _httpClient;

        public AccountController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string fullName)
        {
            // Check if the user already exists by calling the Azure Function
            var existingUserResponse = await _httpClient.GetAsync($"https://cloudfunctionappst10373357.azurewebsites.net/api/getuser/{email}?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D");

            if (existingUserResponse.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Email already exists.");
                return View();
            }

            // Create a new user object
            var user = new Users
            {
                PartitionKey = "Users",
                RowKey = Guid.NewGuid().ToString(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password), // Hash the password
                FullName = fullName,
                CreatedAt = DateTime.UtcNow
            };

            // Serialize the user object to JSON
            var jsonUser = JsonConvert.SerializeObject(user);
            var content = new StringContent(jsonUser, Encoding.UTF8, "application/json");

            // Call the Azure Function to add the user
            var addUserResponse = await _httpClient.PostAsync("https://cloudfunctionappst10373357.azurewebsites.net/api/adduser?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D", content);

            if (addUserResponse.IsSuccessStatusCode)
            {
                return RedirectToAction("Login");
            }

            // Handle the error if the user could not be added
            ModelState.AddModelError("", "An error occurred while registering the user.");
            return View();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            // Log the input values for debugging
            Console.WriteLine($"Email: {email}, Password: {password}");

            // Call the Azure Function to get the user by email
            var userResponse = await _httpClient.GetAsync($"https://cloudfunctionappst10373357.azurewebsites.net/api/getuser/{email}?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D");
            if (!userResponse.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Invalid login attempt. User not found.");
                return View();
            }

            // Deserialize the response content into a User object
            var userJson = await userResponse.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<Users>(userJson);

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid login attempt. Incorrect password.");
                return View();
            }

            // Create an authentication cookie
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            // Add other claims as needed
        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirect to the home page or wherever after login
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet("/test/user/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            // Call the Azure Function to get the user by email
            var userResponse = await _httpClient.GetAsync($"https://cloudfunctionappst10373357.azurewebsites.net/api/getuser/{email}?code=OLTzPj7FCNBOxLgGg49Rusa8LAAWxzq0PM0CT9j3LlfSAzFunp285Q%3D%3D");
            if (!userResponse.IsSuccessStatusCode)
            {
                return NotFound();
            }

            // Deserialize the response content into a User object
            var userJson = await userResponse.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<Users>(userJson);

            return Json(user);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }


    }
}


