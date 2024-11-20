using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace InventorySystem.Controllers
{
    public class ProductController : Controller
    {
        private readonly DbInventoryContext _context;
        public ProductController(DbInventoryContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string searchName, int? categoryId, int? locationId)
        {
            // Obtener todos los productos
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Location)
                .AsQueryable();

            // Filtrar por nombre
            if (!string.IsNullOrEmpty(searchName))
            {
                productsQuery = productsQuery.Where(p => p.ProductName.Contains(searchName));
            }

            // Filtrar por categoría
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.IdCategory == categoryId.Value);
            }

            // Filtrar por ubicación
            if (locationId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.IdLocation == locationId.Value);
            }
            // Pasar datos a la vista
            ViewData["Category"] = new SelectList(_context.Categories, "IdCategory", "CategoryName");
            ViewData["Location"] = new SelectList(_context.Locations, "IdLocation", "LocationName");
           
            var products = await productsQuery.ToListAsync();

            return View(await productsQuery.ToListAsync());
        }
       
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Category"] = new SelectList(_context.Categories, "IdCategory", "CategoryName");
            ViewData["Location"] = new SelectList(_context.Locations, "IdLocation", "LocationName");

            return View();
        }

        [HttpPost]
        //Para asegurar de recibir la informacion de nuestro propio formulario
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model, IFormFile Image)
        {
            if (ModelState.IsValid)
            {
                try
                {
                        var product = new Product()
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
                    if (Image != null)
                    {
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var extension = Path.GetExtension(Image.FileName).ToLower();

                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("Image", "Por favor, sube un archivo de imagen válido (jpg, png, gif).");
                            return View(model);
                        }

                        // Crear un nombre único para la imagen
                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(Image.FileName)}";

                        // Ruta completa del archivo
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);

                        // Guardar la imagen en la carpeta
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await Image.CopyToAsync(stream);
                        }

                        // Guardar la ruta relativa en la base de datos
                        product.ImageRoot = $"/Images/{fileName}";
                    }

                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Captura de errores y registro del mensaje
                    ModelState.AddModelError(string.Empty, $"Error al guardar el producto: {ex.Message}");
                    return View(model);
                }
            }
            else
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Error: {error.ErrorMessage}");
                }
                ViewData["Category"] = new SelectList(_context.Categories, "IdCategory", "CategoryName");
                ViewData["Location"] = new SelectList(_context.Locations, "IdLocation", "LocationName");
                return View(model);
            }
          
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductViewModel
            {
                IdProd= product.IdProd,
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
            ViewData["Category"] = new SelectList(_context.Categories, "IdCategory", "CategoryName");
            ViewData["Location"] = new SelectList(_context.Locations, "IdLocation", "LocationName");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile Image)
        {
            if (Image != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(Image.FileName).ToLower();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("Image", "Por favor, sube un archivo de imagen válido (jpg, png, gif).");
                    return View(product);
                }

                // Crear un nombre único para la imagen
                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(Image.FileName)}";

                // Ruta completa del archivo
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", fileName);

                // Guardar la imagen en la carpeta
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await Image.CopyToAsync(stream);
                }

                // Guardar la ruta relativa en la base de datos
                product.ImageRoot = $"/Images/{fileName}";
            }
            product.LastModDate = DateTime.Now;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                ViewData["Category"] = new SelectList(_context.Categories, "IdCategory", "CategoryName");
                ViewData["Location"] = new SelectList(_context.Locations, "IdLocation", "LocationName");
                return RedirectToAction(nameof(Index));
           
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            Product product = await _context.Products.FirstAsync
                (p => p.IdProd == id);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}
