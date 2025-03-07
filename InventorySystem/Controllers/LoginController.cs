using InventorySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Cryptography;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Models.ViewModels;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using InventorySystem.CommonLib;

namespace InventorySystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly DbInventoryContext _context;
        private readonly IConfiguration _configuration;
        public LoginController(DbInventoryContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegisterAsync(RegisterViewModel user)
        {
            var captchaResponse = Request.Form["g-recaptcha-response"];
            if (string.IsNullOrEmpty(captchaResponse) || !await ValidateCaptcha(captchaResponse))
            {
                return Json(new { success = false, message = "Please solve the reCAPTCHA to continue." });
            }
            // Validate if email is empty
            if (string.IsNullOrWhiteSpace(user.UserMail))
            {
                return Json(new { success = false, message = "Please enter your email address." });
            }

            // Validate email format
            if (!commonLib.IsValidEmail(user.UserMail))
            {
                return Json(new { success = false, message = "The email address is not in a valid format." });
            }

            // Validate if password is empty
            if (string.IsNullOrWhiteSpace(user.UserPassword))
            {
                return Json(new { success = false, message = "Please enter your password." });
            }

            // Validate if passwords match
            if (user.UserPassword != user.ConfirmPassword)
            {
                return Json(new { success = false, message = "The passwords do not match." });
            }
            // Declare parameters
            var mailParam = new SqlParameter("@Mail", user.UserMail);
            var passwordParam = new SqlParameter("@Password", commonLib.ConverterSha256(user.UserPassword));
            var creationDateParam = new SqlParameter("@CreationDate", DateTime.Now);
            var idRolParam = new SqlParameter("@IdRol", 3);
            var registredParam = new SqlParameter("@Registred", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };

            // Execute stored procedure
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_RegisterUser @Mail, @Password, @CreationDate, @IdRol, @Registred OUTPUT, @Message OUTPUT",
                mailParam, passwordParam, creationDateParam, idRolParam, registredParam, messageParam);
            // Read output parameters
            bool result = (bool)registredParam.Value;
            string message = messageParam.Value.ToString();

            if (result)
            {
                return Json(new { success = true, message = "User registered successfully.", redirectUrl = Url.Action("Index", "Login") });
            }
            else
            {
                return Json(new { success = false, message = "This email address has already been registered" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateLogin(LoginViewModel model)
        {
            try
            {
                // Verify if the reCAPTCHA was solved
                var captchaResponse = Request.Form["g-recaptcha-response"];
                if (string.IsNullOrEmpty(captchaResponse) || !await ValidateCaptcha(captchaResponse))
                {
                    return Json(new { success = false, message = "Please solve the reCAPTCHA to continue." });
                }

                // Validate credentials
                if (string.IsNullOrWhiteSpace(model.UserMail) || string.IsNullOrWhiteSpace(model.UserPassword))
                {
                    return Json(new { success = false, message = "Please enter your email and password." });
                }

                model.UserPassword = commonLib.ConverterSha256(model.UserPassword);
                UserLogin result = await _context.UserLogins
                    .FirstOrDefaultAsync(r => r.UserMail == model.UserMail && r.UserPassword == model.UserPassword);

                if (result == null)
                {
                    return Json(new { success = false, message = "Incorrect email or password." });
                }

                // Save session and redirect
                HttpContext.Session.SetInt32("IdUser", result.IdUser);
                HttpContext.Session.SetString("UserMail", result.UserMail);
                HttpContext.Session.SetInt32("IdRol", result.IdRol ?? 0);

                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "An unexpected error occurred. Please try again." });
            }
        }

        private async Task<bool> ValidateCaptcha(string captchaResponse)
        {
            var secretKey = _configuration.GetValue<string>("GoogleReCaptcha:SecretKey");
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={captchaResponse}",
                    null);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                var captchaResult = System.Text.Json.JsonSerializer.Deserialize<CaptchaResult>(jsonResponse);

                return captchaResult != null && captchaResult.success;
            }
        }

        // Class to map the reCAPTCHA response
        private class CaptchaResult
        {
            public bool success { get; set; }
            public string challenge_ts { get; set; } // Challenge timestamp
            public string hostname { get; set; } // Client hostname
        }
    }
}
