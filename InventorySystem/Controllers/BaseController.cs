using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.Controllers
{
    public class BaseController : Controller
    {
        protected readonly IWebHostEnvironment _env;

        public BaseController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index()
        {
            return View();
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

        protected ViewResult SharedView(string folder, string viewName, object model = null)
        {
            var viewPath = $"~/Views/Shared/{folder}/{viewName}.cshtml";
            return View(viewPath, model);
        }

        protected ViewResult SharedProductView(string viewName, object model = null)
        {
            return SharedView("Product", viewName, model);
        }


    }
}