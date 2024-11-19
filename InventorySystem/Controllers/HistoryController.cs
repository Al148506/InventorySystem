using Microsoft.AspNetCore.Mvc;

namespace InventorySystem.Controllers
{
    public class HistoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
