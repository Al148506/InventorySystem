using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
using InventorySystem.Permissions;
using InventorySystem.CommonLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;


namespace InventorySystem.Controllers
{
    public class UserController : Controller
    {
        private readonly DbInventoryContext _context;

        public UserController(DbInventoryContext context)
        {
            _context = context;
        }
        [RoleValidation(1)]
        public async Task<IActionResult> Index()
        {
            // Obtener todos los productos
            var usersQuery = _context.UserLogins
                .Include(p => p.IdRolNavigation)
                .AsQueryable();
            var users = await usersQuery.ToListAsync();

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Rol"] = new SelectList(_context.UserRols, "IdRol", "RolName");

            return View();
        }

        [HttpPost]
        //Para asegurar de recibir la informacion de nuestro propio formulario
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = new UserLogin()
                    {
                        UserName = model.UserName,
                        CreationDate = DateTime.Now,
                        UserMail = model.UserMail,
                        UserPassword = commonLib.ConverterSha256(model.UserPassword),
                        IdRol = model.IdRol
                    };
                    // Validar si las contraseñas coinciden
                    if (model.UserPassword != model.ConfirmPassword)
                    {
                        ViewData["Mensaje"] = "Las contraseñas no coinciden." ;
                        return View(model);
                    }
                    _context.Add(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Captura de errores y registro del mensaje
                    ModelState.AddModelError(string.Empty, $"Error al guardar el usuario: {ex.Message}");
                    return View(model);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }
                return View(model);
            }



        }
    }
}
