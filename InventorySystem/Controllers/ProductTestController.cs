using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace InventorySystem.Controllers
{
    [Route("producttest")]
    public class ProductTestController : BaseController
    {
        private const string SessionKey = "SessionProducts";
        private readonly DbInventoryContext _context;

        public ProductTestController(DbInventoryContext context, IWebHostEnvironment env) : base(env)
        {
            _context = context;
        }

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
            var categories = _context.Categories.ToDictionary(c => c.IdCategory, c => c);
            var locations = _context.Locations.ToDictionary(l => l.IdLocation, l => l);

            var list = new List<Product>
                    {
                        new Product
                        {
                            IdProd = 1,
                            ProductName = "Mesh",
                            Description = "Internet mesh",
                            Quantity = 100,
                            State = "New",
                            IdCategory = 5,
                            CreationDate = new DateTime(2024, 11, 1),
                            LastModDate = new DateTime(2025, 4, 1, 15, 57, 35, 500),
                            ImageRoot = "/Images/20868f37-2fe6-4403-a91f-78e3c8d48530_decox50.jpg",
                            IdLocation = 2,
                            Category = categories.GetValueOrDefault(5),
                            Location = locations.GetValueOrDefault(2)
                        },
                        new Product
                        {
                            IdProd = 2,
                            ProductName = "Iphone 16",
                            Description = "Premium",
                            Quantity = 3,
                            State = "New",
                            IdCategory = 3,
                            CreationDate = new DateTime(2024, 11, 1),
                            LastModDate = new DateTime(2025, 4, 1, 15, 57, 22, 573),
                            ImageRoot = "/Images/iphone16.png",
                            IdLocation = 3,
                            Category = categories.GetValueOrDefault(3),
                            Location = locations.GetValueOrDefault(3)
                        },
                        new Product
                        {
                            IdProd = 3,
                            ProductName = "Iphone Deco X50",
                            Description = "Wifi mesh",
                            Quantity = 4,
                            State = "Good Condition",
                            IdCategory = 3,
                            CreationDate = new DateTime(2024, 11, 1),
                            LastModDate = new DateTime(2024, 11, 1),
                            ImageRoot = "/Images/decox50.jpg",
                            IdLocation = 1,
                            Category = categories.GetValueOrDefault(3),
                            Location = locations.GetValueOrDefault(1)
                        },
                        new Product
                        {
                            IdProd = 4,
                            ProductName = "Iphone Router Huaweii X6",
                            Description = "Black color",
                            Quantity = 3,
                            State = "New",
                            IdCategory = 3,
                            CreationDate = new DateTime(2024, 11, 18, 15, 10, 36),
                            LastModDate = new DateTime(2025, 4, 1, 15, 57, 7, 687),
                            ImageRoot = "/Images/c81700a5-d7df-4d98-afc6-616ddf5c31e8_router.png",
                            IdLocation = 5,
                            Category = categories.GetValueOrDefault(3),
                            Location = locations.GetValueOrDefault(5)
                        },
                        new Product
                        {
                            IdProd = 5,
                            ProductName = "Iphone Xiaomi Poco x4",
                            Description = "Cellphone",
                            Quantity = 4,
                            State = "New",
                            IdCategory = 5,
                            CreationDate = new DateTime(2024, 11, 18, 15, 24, 33),
                            LastModDate = new DateTime(2025, 4, 1, 15, 56, 55, 550),
                            ImageRoot = "/Images/xiaominote10s.jpg",
                            IdLocation = 5,
                            Category = categories.GetValueOrDefault(5),
                            Location = locations.GetValueOrDefault(5)
                        },
                        new Product
                        {
                            IdProd = 7,
                            ProductName = "Iphone Alienware Aurora R16",
                            Description = "Gaming Desktop",
                            Quantity = 3,
                            State = "Excellent",
                            IdCategory = 1,
                            CreationDate = new DateTime(2024, 11, 18, 15, 10, 36),
                            LastModDate = new DateTime(2025, 4, 1, 15, 56, 39, 163),
                            ImageRoot = "/Images/d0654665-0cad-445c-9c01-1bda049d185c_alienware.jpg",
                            IdLocation = 1,
                            Category = categories.GetValueOrDefault(1),
                            Location = locations.GetValueOrDefault(1)
                        }
                    };
            SaveSessionProducts(list);
            return list;
        }

        [HttpGet("index")]
        [HttpGet]
        public async Task<IActionResult> Index(string searchName, int? categoryId, int? locationId, int? numpag, string currentFilter, string currentCategory, string currentLocation,
            string dateFilter, string orderFilter, string currentDate, string currentOrder)
        {
            var products = GetSessionProducts();

            if (!string.IsNullOrEmpty(searchName))
            {
                numpag = 1;
            }
            else
            {
                searchName = currentFilter;
            }
            ViewData["CurrentFilter"] = searchName;
            ViewData["IsTest"] = true;

            if (!string.IsNullOrEmpty(searchName))
            {
                products = products.Where(p => p.ProductName != null && p.ProductName.Contains(searchName, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.IdCategory == categoryId.Value).ToList();
            }

            if (locationId.HasValue)
            {
                products = products.Where(p => p.IdLocation == locationId.Value).ToList();
            }

            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentLocation"] = locationId;
            LoadSelectLists(categoryId, locationId);

            ViewBag.dateFilter = new SelectList(new[]
            {
                        new { Text = "Creation Date", Value = "creation" },
                        new { Text = "Last Modification Date", Value = "modification" }
                    }, "Value", "Text", dateFilter);

            ViewBag.orderFilter = new SelectList(new[]
            {
                        new { Text = "Ascendent Order", Value = "asc" },
                        new { Text = "Descendent Order", Value = "desc" }
                    }, "Value", "Text", orderFilter);

            int regQuantity = 6;
            return SharedProductView("Index", await Pagination<Product>.CreatePagination(products, numpag ?? 1, regQuantity));
        }

        [HttpGet("create")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Category"] = new SelectList(new[] { new { IdCategory = 1, CategoryName = "Accesorios" }, new { IdCategory = 2, CategoryName = "Periféricos" } }, "IdCategory", "CategoryName");
            ViewData["Location"] = new SelectList(new[] { new { IdLocation = 1, LocationName = "Almacén A" }, new { IdLocation = 2, LocationName = "Almacén B" } }, "IdLocation", "LocationName");
            ViewData["State"] = GetStateItems();
            return SharedProductView("Create");
        }
        [HttpPost("create")]
        [HttpPost]
        public async Task<IActionResult> Create(ProductViewModel model, IFormFile? Image)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var products = GetSessionProducts();
                    int nextId = products.Any() ? products.Max(p => p.IdProd) + 1 : 1;
                    var categories = _context.Categories.ToDictionary(c => c.IdCategory, c => c);
                    var locations = _context.Locations.ToDictionary(l => l.IdLocation, l => l);
                    var newProduct = new Product
                    {
                        IdProd = nextId,
                        ProductName = model.ProductName,
                        Description = model.Description,
                        Quantity = model.Quantity,
                        State = model.State,
                        IdCategory = model.IdCategory,
                        CreationDate = DateTime.Now,
                        LastModDate = DateTime.Now,
                        ImageRoot = "/Images/20868f37-2fe6-4403-a91f-78e3c8d48530_decox50.jpg",
                        IdLocation = model.IdLocation,
                        Category = categories.GetValueOrDefault(model.IdCategory),
                        Location = locations.GetValueOrDefault(model.IdLocation)
                    };

                    if (Image != null && await SaveImageAsync(Image) is string imagePath)
                    {
                        newProduct.ImageRoot = imagePath;
                    }

                    products.Add(newProduct);
                    SaveSessionProducts(products);

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error al guardar el producto: {ex.Message}");
                }
            }
            return SharedProductView("Create", model);
        }

     
        [HttpGet("edit/{id}")]
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

            return SharedProductView("Edit", model);
        }

        [HttpPost("edit/{id}")]
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
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("delete/{id}")]
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

            return RedirectToAction(nameof(Index));
        }

        private void LoadSelectLists(int? categoryId = null, int? locationId = null)
        {
            ViewData["Category"] = new SelectList(_context.Categories, "IdCategory", "CategoryName", categoryId);
            ViewData["Location"] = new SelectList(_context.Locations, "IdLocation", "LocationName", locationId);
            ViewData["State"] = GetStateItems();
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
        private async Task<string?> SaveImageAsync(IFormFile image)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(image.FileName).ToLower();

            if (!allowedExtensions.Contains(extension) || !image.ContentType.StartsWith("image/"))
            {
                ModelState.AddModelError("Image", "Por favor, sube un archivo de imagen válido (jpg, png, gif).\n");
                return null;
            }

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);

            using var stream = new FileStream(path, FileMode.Create);
            await image.CopyToAsync(stream);

            return $"/Images/{fileName}";
        }
    }
}