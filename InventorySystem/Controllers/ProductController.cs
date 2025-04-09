using DinkToPdf;
using DinkToPdf.Contracts;
using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace InventorySystem.Controllers
{
    [Route("product")]
    public class ProductController : BaseController
    {
        private readonly DbInventoryContext _context;
        private readonly IConverter _converter;

        public ProductController(DbInventoryContext context, IConverter converter, IWebHostEnvironment env) : base(env)
        {
            _context = context;
            _converter = converter;
        }

        [HttpGet("index")]
        [HttpGet]
        public async Task<IActionResult> Index(string searchName, int? categoryId, int? locationId, int? numpag, string currentFilter, string currentCategory, string currentLocation,
            string dateFilter, string orderFilter, string currentDate, string currentOrder)
        {
            if (Environment.Is64BitProcess) return RedirectToAction("Index", "ProductTest");
            ViewData["Is64Bit"] = Environment.Is64BitProcess;

            var productsQuery = _context.Products.Include(p => p.Category).Include(p => p.Location).AsQueryable();

            if (!string.IsNullOrEmpty(searchName))
            {
                numpag = 1;
            }
            else
            {
                searchName = currentFilter;
            }

            ViewData["CurrentFilter"] = searchName;
            if (!string.IsNullOrEmpty(searchName))
                productsQuery = productsQuery.Where(p => p.ProductName.Contains(searchName));

            if (categoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.IdCategory == categoryId.Value);

            if (locationId.HasValue)
                productsQuery = productsQuery.Where(p => p.IdLocation == locationId.Value);

            if (!string.IsNullOrEmpty(orderFilter) && !string.IsNullOrEmpty(dateFilter))
            {
                productsQuery = (orderFilter, dateFilter) switch
                {
                    ("asc", "creation") => productsQuery.OrderBy(p => p.CreationDate),
                    ("desc", "creation") => productsQuery.OrderByDescending(p => p.CreationDate),
                    ("asc", "modification") => productsQuery.OrderBy(p => p.LastModDate),
                    ("desc", "modification") => productsQuery.OrderByDescending(p => p.LastModDate),
                    _ => productsQuery
                };
            }

            ViewBag.dateFilter = new SelectList(new[] {
                new { Text = "Creation Date", Value = "creation" },
                new { Text = "Last Modification Date", Value = "modification" }
            }, "Value", "Text", dateFilter);

            ViewBag.orderFilter = new SelectList(new[] {
                new { Text = "Ascendent Order", Value = "asc" },
                new { Text = "Descendent Order", Value = "desc" }
            }, "Value", "Text", orderFilter);

            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentLocation"] = locationId;
            ViewData["currentDate"] = dateFilter;
            ViewData["currentOrder"] = orderFilter;

            LoadSelectLists(categoryId, locationId);

            int regQuantity = 6;
            return SharedProductView("Index",await Pagination<Product>.CreatePagination(productsQuery.AsNoTracking(), numpag ?? 1, regQuantity));
        }

        [HttpGet("create")]
        [HttpGet]
        public IActionResult Create()
        {
            if (Environment.Is64BitProcess) return RedirectToAction("Create", "ProductTest");
            LoadSelectLists();
            return SharedProductView("Create");
        }

        [HttpPost("create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model, IFormFile Image)
        {
            if (Environment.Is64BitProcess) return RedirectToAction("Create", "ProductTest");
            LoadSelectLists(model.IdCategory, model.IdLocation);

            if (ModelState.IsValid)
            {
                try
                {
                    var product = new Product
                    {
                        ProductName = model.ProductName,
                        Description = model.Description,
                        Quantity = model.Quantity,
                        State = model.State,
                        IdCategory = model.IdCategory,
                        IdLocation = model.IdLocation,
                        CreationDate = DateTime.Now,
                        LastModDate = DateTime.Now
                    };

                    if (Image != null && await SaveImageAsync(Image) is string imagePath)
                    {
                        product.ImageRoot = imagePath;
                    }

                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Error al guardar el producto: {ex.Message}");
                }
            }

            return SharedProductView("Create",model);
        }

        [HttpGet("edit/{id}")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (Environment.Is64BitProcess) return RedirectToAction("Edit", "ProductTest", new { id = id });

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var model = new ProductViewModel
            {
                IdProd = product.IdProd,
                ProductName = product.ProductName,
                Description = product.Description,
                Quantity = product.Quantity,
                State = product.State,
                IdCategory = product.IdCategory,
                IdLocation = product.IdLocation,
                CreationDate = product.CreationDate,
                LastModDate = product.LastModDate,
                ImageRoot = product.ImageRoot
            };

            LoadSelectLists(product.IdCategory, product.IdLocation);
            return SharedProductView("Edit", model);
        }
        [HttpPost("edit/{id}")]
        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile Image)
        {
            if (Environment.Is64BitProcess) return RedirectToAction("Edit", "ProductTest");

            var existingProduct = await _context.Products.FindAsync(product.IdProd);
            if (existingProduct == null) return NotFound();

            existingProduct.ProductName = product.ProductName;
            existingProduct.Description = product.Description;
            existingProduct.Quantity = product.Quantity;
            existingProduct.State = product.State;
            existingProduct.IdCategory = product.IdCategory;
            existingProduct.IdLocation = product.IdLocation;
            existingProduct.LastModDate = DateTime.Now;

            if (Image != null && await SaveImageAsync(Image) is string imagePath)
            {
                existingProduct.ImageRoot = imagePath;
            }
            LoadSelectLists(product.IdCategory, product.IdLocation);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("delete/{id}")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (Environment.Is64BitProcess) return RedirectToAction("Delete", "ProductTest");

            var product = await _context.Products.FirstOrDefaultAsync(p => p.IdProd == id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private void LoadSelectLists(int? categoryId = null, int? locationId = null)
        {
            ViewData["Category"] = new SelectList(_context.Categories, "IdCategory", "CategoryName", categoryId);
            ViewData["Location"] = new SelectList(_context.Locations, "IdLocation", "LocationName", locationId);
            ViewData["State"] = GetStateItems();
        }

        private List<SelectListItem> GetStateItems() => new List<SelectListItem>
        {
            new SelectListItem { Text = "New", Value = "New" },
            new SelectListItem { Text = "Excellent", Value = "Excellent" },
            new SelectListItem { Text = "Very Good", Value = "Very Good" },
            new SelectListItem { Text = "Good", Value = "Good" },
            new SelectListItem { Text = "Used", Value = "Used" },
            new SelectListItem { Text = "For parts or not working", Value = "For parts or not working" }
        };

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
