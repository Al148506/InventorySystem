using InventorySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Controllers
{
    public class ChangeLogController : Controller
    {
        private readonly DbInventoryContext _context;
        public ChangeLogController(DbInventoryContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var history = _context.ChangeLogs
           .OrderByDescending(log => log.DateMod)
           .ToList();

            return View(history);
        }
    }
}
