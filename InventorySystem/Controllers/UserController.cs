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
                    ViewData["Rol"] = new SelectList(_context.UserRols, "IdRol", "RolName");
                    var user = new UserLogin()
                    {
                        UserName = model.UserName,
                        CreationDate = DateTime.Now,
                        UserMail = model.UserMail,
                        UserPassword = commonLib.ConverterSha256(model.UserPassword),
                        IdRol = model.IdRol
                    };
                    if (string.IsNullOrWhiteSpace(model.UserMail) || !commonLib.IsValidEmail(model.UserMail))
                    {
                        ViewData["Mensaje"] = "Introduzca un correo valido";
                        return View(model);
                    }
                    if (string.IsNullOrWhiteSpace(model.UserPassword))
                    {
                        ViewData["Mensaje"] = "Introduzca una contraseña valida";
                        return View(model);
                    }
                    if (model.UserPassword != model.ConfirmPassword)
                    {
                        ViewData["Mensaje"] = "Las contraseñas no coinciden.";
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
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.UserLogins.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewData["Rol"] = new SelectList(_context.UserRols, "IdRol", "RolName");
            var model = new RegisterViewModel
            {
                IdUser = user.IdUser,
                UserMail = user.UserMail,
                UserPassword = user.UserPassword,
                UserName = user.UserName,
                IdRol = user.IdRol,
                CreationDate = user.CreationDate,
                LastModDate = DateTime.Now
            };
            if (string.IsNullOrWhiteSpace(model.UserMail) || !commonLib.IsValidEmail(model.UserMail))
            {
                ViewData["Mensaje"] = "Introduzca un correo valido";
                return View(model);
            }
            if (string.IsNullOrWhiteSpace(model.UserPassword))
            {
                ViewData["Mensaje"] = "Introduzca una contraseña valida";
                return View(model);
            }
            if (model.UserPassword != model.ConfirmPassword)
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden.";
                return View(model);
            }
           
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserLogin user)
        {
            var existingUser = await _context.UserLogins.AsNoTracking().FirstOrDefaultAsync(u => u.IdUser == user.IdUser);
            if (existingUser == null)
            {
                return NotFound();
            }
            // Mantiene el valor original de CreationDate
            user.CreationDate = existingUser.CreationDate;
            user.UserPassword = commonLib.ConverterSha256(user.UserPassword);
            _context.UserLogins.Update(user);
            await _context.SaveChangesAsync();
            ViewData["Rol"] = new SelectList(_context.UserRols, "IdRol", "RolName");
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            UserLogin user = await _context.UserLogins.FirstAsync
                (p => p.IdUser == id);
            _context.UserLogins.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}