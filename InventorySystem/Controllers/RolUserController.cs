using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Controllers
{
    public class RolUserController : Controller
    {
        private readonly DbContext _context;
        public RolUserController(DbContext _context)
        {
            _context = _context ?? throw new ArgumentNullException("Failed to connect Database");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
