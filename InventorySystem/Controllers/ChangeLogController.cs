using InventorySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventorySystem.Controllers
{
    public class ChangeLogController : Controller
    {
        private static List<ChangeLog> _changeLogs = new List<ChangeLog>();

        public ChangeLogController()
        {
            // Inicialización con datos ficticios (si es necesario)
            if (!_changeLogs.Any())
            {
                _changeLogs.Add(new ChangeLog { Id = 1, UserId = "User1", TypeAction = "Create", DateMod = DateTime.Now });
                _changeLogs.Add(new ChangeLog { Id = 2, UserId = "User2", TypeAction = "Update", DateMod = DateTime.Now.AddMinutes(-30) });
            }
        }

        public IActionResult Index(string searchName, string orderFilter, int? numpag, string currentFilter, string currentOrder, string actionType, string currentActionType)
        {
            ViewData["Is64Bit"] = Environment.Is64BitProcess;
            var logsQuery = _changeLogs.AsQueryable();

            // Obtener valores únicos de ActionType
            var actionTypes = _changeLogs.Select(log => log.TypeAction).Distinct().OrderBy(type => type).ToList();
            ViewBag.typeAction = new SelectList(actionTypes);

            // Filtrado por UserId
            if (!string.IsNullOrEmpty(searchName))
            {
                numpag = 1;
                logsQuery = logsQuery.Where(p => p.UserId.Contains(searchName));
            }
            else
            {
                searchName = currentFilter;
            }
            ViewData["CurrentFilter"] = searchName;

            // Filtrado por ActionType
            if (!string.IsNullOrEmpty(actionType))
            {
                logsQuery = logsQuery.Where(p => p.TypeAction == actionType);
                ViewData["currentActionType"] = actionType;
            }
            else
            {
                actionType = currentActionType;
                ViewData["currentActionType"] = currentActionType;
            }

            // Orden dinámico
            if (!string.IsNullOrEmpty(orderFilter))
            {
                logsQuery = orderFilter == "asc" ? logsQuery.OrderBy(p => p.DateMod) : logsQuery.OrderByDescending(p => p.DateMod);
            }
            else
            {
                orderFilter = currentOrder;
            }
            ViewData["CurrentOrder"] = orderFilter;

            // Opciones de orden
            ViewBag.orderFilter = new SelectList(new[]
            {
                new { Text = "Ascendent Order", Value = "asc" },
                new { Text = "Descendent Order", Value = "desc" }
            }, "Value", "Text", orderFilter);

            int regQuantity = 5;
            return View(logsQuery.Take(regQuantity).ToList()); // Simulación de paginación
        }
    }
}