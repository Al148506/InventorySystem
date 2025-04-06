using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.Controllers
{
    public class BaseController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        protected readonly IWebHostEnvironment _env;

        public BaseController(IWebHostEnvironment env)
        {
            _env = env;
        }

        protected bool IsStaging()
        {
            return _env.EnvironmentName == "Staging";
        }

        protected bool IsDevelopment()
        {
            return _env.EnvironmentName == "Development";
        }

        protected bool IsProduction()
        {
            return _env.EnvironmentName == "Production";
        }
    }
}