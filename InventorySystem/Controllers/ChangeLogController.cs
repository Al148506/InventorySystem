using InventorySystem.Data;
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
        public async Task<IActionResult> Index(int? numpag)
        {
            var logsQuery = _context.ChangeLogs
           .OrderByDescending(log => log.DateMod)
           .AsQueryable();

            int regQuantity = 5;
            return View(await Pagination<ChangeLog>.CreatePagination(logsQuery.AsNoTracking(), numpag ?? 1, regQuantity));
        }
    }
}
