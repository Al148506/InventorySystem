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
        public IActionResult Register(UserLoginViewModel user) 
        {
   
            if (user.UserPassword == user.ConfirmPassword)
            {
                user.UserPassword = ConverterSha256(user.UserPassword);
            }
            else
            {
                ViewData["Mensaje"] = "Passwords do NOT match";
                return View();
            }
            // Declarar parámetros
            var mailParam = new SqlParameter("@Mail", user.UserMail);
            var passwordParam = new SqlParameter("@Password", user.UserPassword);
            var creationDateParam = new SqlParameter("@CreationDate", DateTime.Now);
            var registredParam = new SqlParameter("@Registred", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };

            // Ejecutar procedimiento
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_RegisterUser @Mail, @Password,@CreationDate, @Registred OUTPUT, @Message OUTPUT",
                mailParam, passwordParam, creationDateParam, registredParam, messageParam);
            // Leer parámetros de salida
            bool registred = (bool)registredParam.Value;
            string message = messageParam.Value.ToString();

            ViewData["Mensaje"] = message;
            if (registred)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                return View();
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
                if (!ModelState.IsValid)
                {
                    // Si el modelo no es válido, regresar la vista con los errores
                    return View("Index", model);
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

                // Redirigir al inicio
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });

            }
            catch (Exception ex)
            {
                // Manejar errores
                return Json(new { success = false, message = "Ocurrió un error inesperado. Inténtalo más tarde." });
            }
        }


        /* // Declarar parámetros
         var mailParam = new SqlParameter("@Mail", user.UserMail);
         var passwordParam = new SqlParameter("@Password", user.UserPassword);

         // Ejecutar procedimiento
         var result = _context.Set<UserValidationResultDTO>()
         .FromSqlInterpolated($"EXEC sp_UserValidation @UserMail = {mailParam}, @UserPassword = {passwordParam}")
         .AsEnumerable()
         .FirstOrDefault();
         if (result != null && result.IdUser != 0) // Comprobar si el resultado no es "0"
         {
             // Usuario válido, redirigir a la página principal
             HttpContext.Session.SetInt32("IdUser", result.IdUser);
             HttpContext.Session.SetString("UserMail", user.UserMail);
             return RedirectToAction("Index", "Home");
         }
         else
         {
             // Usuario no válido, mostrar mensaje de error
             ViewData["Mensaje"] = "Correo o contraseña incorrectos.";
             return View("Index");
         }*/
    
}
}
