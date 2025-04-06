using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace InventorySystem.Controllers
{
    public class ProductTestController : Controller
    {
        private const string SessionKey = "SessionProducts";

        private List<Product> GetSessionProducts()
        {
            var json = HttpContext.Session.GetString(SessionKey);
            return string.IsNullOrEmpty(json) ? GetInitialProducts() : JsonConvert.DeserializeObject<List<Product>>(json) ?? new List<Product>();
        }

        private void SaveSessionProducts(List<Product> products)
        {
            HttpContext.Session.SetString(SessionKey, JsonConvert.SerializeObject(products));
        }

        private List<Product> GetInitialProducts()
        {
            var list = new List<Product>
                {
                    new Product { IdProd = 1, ProductName = "Mouse", Quantity = 10, State = "New", Description = "Mouse óptico", IdCategory = 1, IdLocation = 1, CreationDate = DateTime.Now },
                    new Product { IdProd = 2, ProductName = "Teclado", Quantity = 5, State = "Used", Description = "Teclado mecánico", IdCategory = 2, IdLocation = 2, CreationDate = DateTime.Now }
                };
            SaveSessionProducts(list);
            return list;
        }

        public async Task<IActionResult> Index(string searchName, int? categoryId, int? locationId, int? numpag, string currentFilter, string currentCategory, string currentLocation)
        {
            var products = GetSessionProducts();
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
            // Filtrar por nombre
            if (!string.IsNullOrEmpty(searchName))
            {
                products = products.Where(p => p.ProductName.Contains(searchName)).ToList();
            }

            // Filtrar por categoría
            if (categoryId.HasValue)
            {
                products = products.Where(p => p.IdCategory == categoryId.Value).ToList();
            }

            // Filtrar por ubicación
            if (locationId.HasValue)
            {
                products = products.Where(p => p.IdLocation == locationId.Value).ToList();
            }
            //Mantiene los filtros durante paginacion
            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentLocation"] = locationId;

            // Pasar datos a la vista
            ViewData["Category"] = new SelectList(new[] { new { IdCategory = 1, CategoryName = "Accesorios" }, new { IdCategory = 2, CategoryName = "Periféricos" } }, "IdCategory", "CategoryName");
            ViewData["Location"] = new SelectList(new[] { new { IdLocation = 1, LocationName = "Almacén A" }, new { IdLocation = 2, LocationName = "Almacén B" } }, "IdLocation", "LocationName");

            //var products = await productsQuery.ToListAsync();
            int regQuantity = 6;
            return View(await Pagination<Product>.CreatePagination(products, numpag ?? 1, regQuantity));
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Category"] = new SelectList(new[] { new { IdCategory = 1, CategoryName = "Accesorios" }, new { IdCategory = 2, CategoryName = "Periféricos" } }, "IdCategory", "CategoryName");
            ViewData["Location"] = new SelectList(new[] { new { IdLocation = 1, LocationName = "Almacén A" }, new { IdLocation = 2, LocationName = "Almacén B" } }, "IdLocation", "LocationName");
            ViewData["State"] = GetStateItems();
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                var products = GetSessionProducts();
                int nextId = products.Any() ? products.Max(p => p.IdProd) + 1 : 1;

                var newProduct = new Product
                {
                    IdProd = nextId,
                    ProductName = model.ProductName,
                    Description = model.Description,
                    Quantity = model.Quantity,
                    State = model.State,
                    IdCategory = model.IdCategory,
                    IdLocation = model.IdLocation,
                    CreationDate = DateTime.Now
                };

                products.Add(newProduct);
                SaveSessionProducts(products);

                return RedirectToAction("Index");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var products = GetSessionProducts();
            var product = products.FirstOrDefault(p => p.IdProd == id);

            if (product == null)
                return NotFound();

            var model = new ProductViewModel
            {
                IdProd = product.IdProd,
                ProductName = product.ProductName,
                Description = product.Description,
                Quantity = product.Quantity,
                State = product.State,
                IdCategory = product.IdCategory,
                IdLocation = product.IdLocation,
                CreationDate = product.CreationDate
            };

            ViewData["Category"] = new SelectList(new[] { new { IdCategory = 1, CategoryName = "Accesorios" }, new { IdCategory = 2, CategoryName = "Periféricos" } }, "IdCategory", "CategoryName");
            ViewData["Location"] = new SelectList(new[] { new { IdLocation = 1, LocationName = "Almacén A" }, new { IdLocation = 2, LocationName = "Almacén B" } }, "IdLocation", "LocationName");
            ViewData["State"] = GetStateItems();

            return View(model);
        }

        [HttpPost]
        public IActionResult Edit(ProductViewModel model)
        {
            var products = GetSessionProducts();
            var product = products.FirstOrDefault(p => p.IdProd == model.IdProd);
            if (product == null)
                return NotFound();

            product.ProductName = model.ProductName;
            product.Description = model.Description;
            product.Quantity = model.Quantity;
            product.State = model.State;
            product.IdCategory = model.IdCategory;
            product.IdLocation = model.IdLocation;
            product.LastModDate = DateTime.Now;

            SaveSessionProducts(products);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var products = GetSessionProducts();
            var product = products.FirstOrDefault(p => p.IdProd == id);
            if (product != null)
            {
                products.Remove(product);
                SaveSessionProducts(products);
            }

            return RedirectToAction("Index");
        }

        private List<SelectListItem> GetStateItems()
        {
            return new List<SelectListItem>
                {
                    new SelectListItem { Text = "New", Value = "New" },
                    new SelectListItem { Text = "Excellent", Value = "Excellent" },
                    new SelectListItem { Text = "Very Good", Value = "Very Good" },
                    new SelectListItem { Text = "Good", Value = "Good" },
                    new SelectListItem { Text = "Used", Value = "Used" },
                    new SelectListItem { Text = "For parts or not working", Value = "For parts or not working" }
                };
        }
    }
}
