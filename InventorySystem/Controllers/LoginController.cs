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
        public IActionResult Register(UserLogin user) 
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
            var registredParam = new SqlParameter("@Registred", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", SqlDbType.VarChar, 100) { Direction = ParameterDirection.Output };

            // Ejecutar procedimiento
            _context.Database.ExecuteSqlRaw(
                "EXEC sp_RegisterUser @Mail, @Password, @Registred OUTPUT, @Message OUTPUT",
                mailParam, passwordParam, registredParam, messageParam);
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
        public IActionResult ValidateLogin(string Mail, string Password)
        {
            // Verificar si existe un usuario con las credenciales proporcionadas
            var user = _context.UserLogins
                .FirstOrDefault(u => u.UserMail == Mail && u.UserPassword == Password);

            if (user != null)
            {
                // Usuario encontrado: redirigir al área principal
               // TempData["UserName"] = user.UserName;  Ejemplo: pasar datos entre vistas
                return RedirectToAction("Index", "Home");
            }
            else
            {
                // Usuario no encontrado: mostrar error
                ViewData["Mensaje"] = "Credenciales incorrectas. Inténtalo de nuevo.";
                return View("Index");
            }
        }
    }
}
