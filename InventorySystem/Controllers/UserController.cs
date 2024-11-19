using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
