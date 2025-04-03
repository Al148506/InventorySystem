using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
using InventorySystem.Permissions;
using InventorySystem.CommonLib;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;
using DinkToPdf;
using DinkToPdf.Contracts;

namespace InventorySystem.Controllers
{
    [RoleValidation(1)]
    public class UserController : Controller
    {
        private readonly IConverter _converter;
        private static Dictionary<int, UserLogin> _userLogins = new();
        private static Dictionary<int, UserRol> _userRoles = new()
        {
            { 1, new UserRol { IdRol = 1, RolName = "Admin" } },
            { 2, new UserRol { IdRol = 2, RolName = "User" } }
        };

        public UserController(IConverter converter)
        {
            _converter = converter;
        }

        [HttpGet]
        public IActionResult Index(string searchName)
        {
            var users = _userLogins.Values.AsQueryable();
            if (!string.IsNullOrEmpty(searchName))
            {
                users = users.Where(u => u.UserName.Contains(searchName));
            }
            return View(users.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Rol"] = new SelectList(_userRoles.Values, "IdRol", "RolName", 3);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_userLogins.Values.Any(u => u.UserMail == model.UserMail))
                {
                    ViewData["Mensaje"] = "This email is already registered. Please choose another one";
                    return View(model);
                }
                var user = new UserLogin
                {
                    IdUser = _userLogins.Count + 1,
                    UserName = model.UserName,
                    UserMail = model.UserMail,
                    UserPassword = commonLib.ConverterSha256(model.UserPassword),
                    IdRol = model.IdRol,
                    CreationDate = DateTime.Now
                };
                _userLogins[user.IdUser] = user;
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!_userLogins.TryGetValue(id, out var user))
                return NotFound();

            var model = new RegisterViewModel
            {
                IdUser = user.IdUser,
                UserMail = user.UserMail,
                UserName = user.UserName,
                IdRol = user.IdRol,
                CreationDate = user.CreationDate
            };
            ViewData["Rol"] = new SelectList(_userRoles.Values, "IdRol", "RolName", user.IdRol);
            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(UserLogin user)
        {
            if (!_userLogins.ContainsKey(user.IdUser))
                return NotFound();

            _userLogins[user.IdUser] = user;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            _userLogins.Remove(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
