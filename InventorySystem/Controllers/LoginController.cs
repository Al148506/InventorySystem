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
                return Json(new { success = false, message = "Por favor, resuelva el reCAPTCHA para continuar." });
            }
            // Validar si el correo está vacío
            if (string.IsNullOrWhiteSpace(user.UserMail))
            {
                return Json(new { success = false, message = "Por favor, ingrese su correo electrónico." });
            }

            // Validar formato del correo
            if (!commonLib.IsValidEmail(user.UserMail))
            {
                return Json(new { success = false, message = "El correo electrónico no tiene un formato válido." });
            }

            // Validar si la contraseña está vacía
            if (string.IsNullOrWhiteSpace(user.UserPassword))
            {
                return Json(new { success = false, message = "Por favor, ingrese su contraseña." });
            }

            // Validar si las contraseñas coinciden
            if (user.UserPassword != user.ConfirmPassword)
            {
                return Json(new { success = false, message = "Las contraseñas no coinciden." });
            }
            // Declarar parámetros
            var mailParam = new SqlParameter("@Mail", user.UserMail);
            var passwordParam = new SqlParameter("@Password", commonLib.ConverterSha256(user.UserPassword));
            var creationDateParam = new SqlParameter("@CreationDate", DateTime.Now);
            var idRolParam = new SqlParameter("@IdRol", 3);
            var registredParam = new SqlParameter("@Registred", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };

            // Ejecutar procedimiento
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_RegisterUser @Mail, @Password,@CreationDate,@IdRol, @Registred OUTPUT, @Message OUTPUT",
                mailParam, passwordParam, creationDateParam,idRolParam, registredParam, messageParam);
            // Leer parámetros de salida
            bool result = (bool)registredParam.Value;
            string message = messageParam.Value.ToString();

            if (result)
            {

                return Json(new { success = true, message = "Usuario registrado exitosamente.", redirectUrl = Url.Action("Index", "Login") });
            }
            else
            {
                return Json(new { success = false, message = "Este correo ya ha sido registrado" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ValidateLogin(LoginViewModel model)
        {
            try
            {
                // Verificar si el reCAPTCHA fue resuelto
                var captchaResponse = Request.Form["g-recaptcha-response"];
                if (string.IsNullOrEmpty(captchaResponse) || !await ValidateCaptcha(captchaResponse))
                {
                    return Json(new { success = false, message = "Por favor, resuelva el reCAPTCHA para continuar." });
                }

                // Lógica existente para validar credenciales
                if (string.IsNullOrWhiteSpace(model.UserMail) || string.IsNullOrWhiteSpace(model.UserPassword))
                {
                    return Json(new { success = false, message = "Por favor, ingrese su correo y contraseña." });
                }

                model.UserPassword = commonLib.ConverterSha256(model.UserPassword);
                UserLogin result = await _context.UserLogins
                    .FirstOrDefaultAsync(r => r.UserMail == model.UserMail && r.UserPassword == model.UserPassword);

                if (result == null)
                {
                    return Json(new { success = false, message = "Correo o contraseña incorrectos." });
                }

                // Guardar sesión y redirigir
                HttpContext.Session.SetInt32("IdUser", result.IdUser);
                HttpContext.Session.SetString("UserMail", result.UserMail);
                HttpContext.Session.SetInt32("IdRol", result.IdRol ?? 0);

                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Ocurrió un error inesperado. Inténtelo de nuevo." });
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


        // Clase para mapear la respuesta del reCAPTCHA
        private class CaptchaResult
        {
            public bool success { get; set; }
            public string challenge_ts { get; set; } // Marca de tiempo del desafío
            public string hostname { get; set; } // Nombre del host del cliente
        }


    }
}
