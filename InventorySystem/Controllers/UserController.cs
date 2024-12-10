using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
using InventorySystem.Permissions;
using InventorySystem.CommonLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;


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
        [HttpGet]
        public async Task<IActionResult> Index(string searchName, string dateFilter, string orderFilter, int? numpag, string currentFilter,string currentOrder)
        {
            // Obtener todos los usuarios con sus relaciones necesarias
            var usersQuery = _context.UserLogins
                .Include(p => p.IdRolNavigation)
                .AsQueryable();
            if (!string.IsNullOrEmpty(searchName))
            {
                numpag = 1;
            }
            else
            {
                searchName = currentFilter;
            }
            ViewData["CurrentFilter"] = searchName; // Asegúrate de actualizar el filtro en ViewData
           

            // Aplicar filtro de búsqueda por nombre
            if (!string.IsNullOrEmpty(searchName))
            {
                usersQuery = usersQuery.Where(p => p.UserName.Contains(searchName));
            }

            // Aplicar ordenamiento dinámico
            if (!string.IsNullOrEmpty(orderFilter) && !string.IsNullOrEmpty(dateFilter))
            {
                usersQuery = (orderFilter, dateFilter) switch
                {
                    ("asc", "creation") => usersQuery.OrderBy(p => p.CreationDate),
                    ("desc", "creation") => usersQuery.OrderByDescending(p => p.CreationDate),
                    ("asc", "modification") => usersQuery.OrderBy(p => p.LastModDate),
                    ("desc", "modification") => usersQuery.OrderByDescending(p => p.LastModDate),
                    _ => usersQuery // Mantener sin cambios si no coincide ningún filtro
                };
            }
            ViewData["CurrentOrder"] = orderFilter;
            ViewData["CurrentDateFilter"] = dateFilter;
            // Preparar las listas para los SelectList conservando valores seleccionados
            ViewBag.dateFilter = new SelectList(new[]
            {
        new { Text = "Creation Date", Value = "creation" },
        new { Text = "Last Modification Date", Value = "modification" }
    }, "Value", "Text", dateFilter); // Selección actual

            ViewBag.orderFilter = new SelectList(new[]
            {
        new { Text = "Ascendent Order", Value = "asc" },
        new { Text = "Descendent Order", Value = "desc" }
    }, "Value", "Text", orderFilter); // Selección actual

            //var users = await usersQuery.ToListAsync();
            int regQuantity = 6;
            return View(await Pagination<UserLogin>.CreatePagination(usersQuery.AsNoTracking(), numpag ?? 1, regQuantity));
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