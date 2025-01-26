using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
using InventorySystem.Permissions;
using InventorySystem.CommonLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InventorySystem.Data;
using DinkToPdf;
using System.Text;
using DinkToPdf.Contracts;
using System;


namespace InventorySystem.Controllers
{
    [RoleValidation(1)]
    public class UserController : Controller
    {
        private readonly DbInventoryContext _context;
        private readonly IConverter _converter;
        public UserController(DbInventoryContext context, IConverter converter)
        {
            _context = context;
            _converter = converter;
        }
        [HttpGet]
        public async Task<IActionResult> Index(string searchName, string dateFilter, string orderFilter, int? numpag, string currentFilter, string currentOrder)
        {
            ViewData["Is64Bit"] = Environment.Is64BitProcess;
            // Obtener todos los usuarios con sus relaciones necesarias
            var usersQuery = _context.UserLogins
                .Include(p => p.IdRolNavigation)
                .AsQueryable();
            //Paginacion
            if (!string.IsNullOrEmpty(searchName))
            {
                numpag = 1;
            }
            else
            {
                searchName = currentFilter;
            }
            ViewData["CurrentFilter"] = searchName;


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
            ViewData["Rol"] = new SelectList(_context.UserRols, "IdRol", "RolName", 3);

            return View();
        }

        [HttpPost]
        //Para asegurar de recibir la informacion de nuestro propio formulario
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegisterViewModel model)
        {
            ViewData["Rol"] = new SelectList(_context.UserRols, "IdRol", "RolName", 3);
            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar si el correo ya existe
                    var emailExists = await _context.UserLogins.AnyAsync(u => u.UserMail == model.UserMail);
                    if (emailExists)
                    {
                        ViewData["Mensaje"] = "This email is already registered. Please choose another one";
                        return View(model);
                    }
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
                        ViewData["Mensaje"] = "Please enter a valid email";
                        return View(model);
                    }
                    if (string.IsNullOrWhiteSpace(model.UserPassword))
                    {
                        ViewData["Mensaje"] = "Please enter a valid password";
                        return View(model);
                    }
                    if (model.UserPassword != model.ConfirmPassword)
                    {
                        ViewData["Mensaje"] = "Passwords do not match ";
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
            user.UserPassword = existingUser.UserPassword;
            //user.UserPassword = commonLib.ConverterSha256(user.UserPassword);
            try
            {
                _context.UserLogins.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Registra el error y devuelve un mensaje al usuario
                ModelState.AddModelError("", "No se pudo actualizar el usuario.");
            }
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

        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            var user = await _context.UserLogins.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var model = new ChangePasswordViewModel
            {
                IdUser = user.IdUser
            };
            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _context.UserLogins.FindAsync(model.IdUser);
            if (user == null)
            {
                return NotFound();
            }
            var confirmPasswordHash = commonLib.ConverterSha256(model.ConfirmPassword);
            user.UserPassword = commonLib.ConverterSha256(model.NewPassword);
            user.LastModDate = DateTime.Now;
            if (user.UserPassword != confirmPasswordHash)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(model);
            }
          
            try
            {
                _context.UserLogins.Update(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "La contraseña se ha cambiado correctamente.";
                return RedirectToAction("Index", "User");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "No se pudo actualizar la contraseña.");
                return View(model);

            }
        }

        public IActionResult GeneratePdf()
        {
            // Obtener todos los datos de la tabla ChangeLog
            var users = _context.UserLogins
                .Include(u => u.IdRolNavigation)
                .AsNoTracking()
                .ToList();


            // Construir el contenido HTML para el PDF usando StringBuilder para mejorar el rendimiento
            var htmlContent = new StringBuilder();
            htmlContent.Append(@"
        <html>
        <head>
            <style>
                body {
                    font-family: Arial, sans-serif;
                    margin: 0;
                    padding: 0;
                }
                h1 {
                    text-align: center;
                    font-size: 24px;
                    margin-bottom: 20px;
                }
                table {
                    width: 100%;
                    border-collapse: collapse;
                    table-layout: fixed;
                }
                th, td {
                    border: 1px solid #ddd;
                    padding: 8px;
                    text-align: left;
                    word-wrap: break-word; /* Evita desbordamientos en las celdas */
                }
                th {
                    background-color: #f2f2f2;
                    font-weight: bold;
                }
                tr {
                    page-break-inside: avoid; /* Evita que una fila se corte entre páginas */
                }
            </style>
        </head>
        <body>
            <h1>Users Report</h1>
            <table>
                <thead>
                    <tr>
                        <th>Id</th>
                        <th>UserName</th>
                        <th>UserMail</th>
                        <th>Rol</th>
                        <th>CreationDate</th>
                        <th>LastModDate</th>
                    </tr>
                </thead>
                <tbody>");

            foreach (var user in users)
            {
                htmlContent.Append("<tr>")
                           .AppendFormat("<td>{0}</td>", user.IdUser)
                           .AppendFormat("<td>{0}</td>", user.UserName)
                           .AppendFormat("<td>{0}</td>", user.UserMail)
                           .AppendFormat("<td>{0}</td>", user?.IdRolNavigation?.RolName ?? "N/A")
                           .AppendFormat("<td>{0:yyyy-MM-dd HH:mm:ss}</td>", user.CreationDate)
                           .AppendFormat("<td>{0}</td>", user.LastModDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Whithout modifications")
                           .Append("</tr>");
            }

            htmlContent.Append(@"
                </tbody>
            </table>
        </body>
        </html>");

            // Configurar el documento PDF
            var pdfDoc = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Landscape,
                    PaperSize = PaperKind.A4,
                    Margins = new MarginSettings { Top = 10, Bottom = 10, Left = 10, Right = 10 },
                }
            };

            pdfDoc.Objects.Add(new ObjectSettings
            {
                HtmlContent = htmlContent.ToString(),
                WebSettings = { DefaultEncoding = "utf-8" }
            });

            // Convertir a PDF
            var pdf = _converter.Convert(pdfDoc);

            // Retornar el PDF como archivo descargable
            return File(pdf, "application/pdf", "ChangeLog.pdf");
        }

    }
}