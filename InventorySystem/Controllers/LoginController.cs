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
using InventorySystem.Models.DTOs;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace InventorySystem.Controllers
{
    public class LoginController : Controller
    {
        private readonly DbInventoryContext _context;
        public LoginController(DbInventoryContext context)
        {
            _context = context;
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
        public IActionResult Register(RegisterViewModel user) 
        {
            // Validar si el correo está vacío
            if (string.IsNullOrWhiteSpace(user.UserMail))
            {
                return Json(new { success = false, message = "Por favor, ingrese su correo electrónico." });
            }

            // Validar formato del correo
            if (!IsValidEmail(user.UserMail))
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
            var passwordParam = new SqlParameter("@Password", ConverterSha256(user.UserPassword));
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


        public static string ConverterSha256(string texto)
        {

            StringBuilder Sb = new StringBuilder();
            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                byte[] result = hash.ComputeHash(enc.GetBytes(texto));

                foreach (byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }



        [HttpPost]
        public async Task<IActionResult> ValidateLogin(LoginViewModel model)
        {
            try
            {
                // Verificar si los campos están vacíos
                if (string.IsNullOrWhiteSpace(model.UserMail) && string.IsNullOrWhiteSpace(model.UserPassword))
                {
                    return Json(new { success = false, message = "Por favor, ingrese el correo y la contraseña." });
                }

                if (string.IsNullOrWhiteSpace(model.UserMail))
                {
                    return Json(new { success = false, message = "Por favor, ingrese su correo electrónico." });
                }

                if (string.IsNullOrWhiteSpace(model.UserPassword))
                {
                    return Json(new { success = false, message = "Por favor, ingrese su contraseña." });
                }
                // Convertir contraseña a SHA-256
                model.UserPassword = ConverterSha256(model.UserPassword);

                // Buscar usuario en la base de datos
                UserLogin result = await _context.UserLogins
                    .FirstOrDefaultAsync(r => r.UserMail == model.UserMail && r.UserPassword == model.UserPassword);

                if (result == null)
                {
                    // Usuario no encontrado
                    return Json(new { success = false, message = "Correo o contraseña incorrectos." });
                }

                // Guardar datos en la sesión
                HttpContext.Session.SetInt32("IdUser", result.IdUser);
                HttpContext.Session.SetString("UserMail", result.UserMail);
                if (result.IdRol != null)
                {
                    // Asignar el valor de IdRol a la sesión si no es null
                    HttpContext.Session.SetInt32("IdRol", result.IdRol.Value); // Usa .Value porque IdRol es probablemente un int? (nullable)
                }
                else
                {
                    // Manejar el caso cuando IdRol es null
                    return Json(new { success = false, message = "El rol del usuario no está definido. Comuníquese con el administrador." });
                }

                // Redirigir al inicio
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });

            }
            catch (Exception ex)
            {
                // Manejar errores inesperados
                Console.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "Ocurrió un error inesperado. Inténtalo nuevamente." });
            }
        }
        // Método para validar formato de correo electrónico
        private bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$");
            return emailRegex.IsMatch(email);
        }
    }
}
