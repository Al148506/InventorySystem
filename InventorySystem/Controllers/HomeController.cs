using InventorySystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace InventorySystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult AccessDenied()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult CloseSession()
        {
            // Limpiar toda la sesi�n
            HttpContext.Session.Clear();

            // Opcional: Redirigir al usuario a la p�gina de inicio de sesi�n o de inicio
            return RedirectToAction("Index", "Login");
        }
        
    }
}
