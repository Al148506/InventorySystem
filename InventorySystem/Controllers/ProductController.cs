using DinkToPdf;
using DinkToPdf.Contracts;
using InventorySystem.Data;
using InventorySystem.Models;
using InventorySystem.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace InventorySystem.Controllers
{
    public class ProductController : Controller
    {
        private readonly DbInventoryContext _context;
        private readonly IConverter _converter;
        public ProductController(DbInventoryContext context, IConverter converter)
        {
            _context = context;
            _converter = converter;
        }
        public async Task<IActionResult> Index(string searchName, int? categoryId, int? locationId, int? numpag, string currentFilter, string currentCategory, string currentLocation)
        {

            ViewData["Is64Bit"] = Environment.Is64BitProcess;
            // Obtener todos los productos
            var productsQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Location)
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
            //Mantiene los filtros durante paginacion
            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentLocation"] = locationId;

            // Pasar datos a la vista
            ViewData["Category"] = new SelectList(_context.Categories, "IdCategory", "CategoryName",categoryId);
            ViewData["Location"] = new SelectList(_context.Locations, "IdLocation", "LocationName",locationId);

            //var products = await productsQuery.ToListAsync();
            int regQuantity = 6;
            return View(await Pagination<Product>.CreatePagination(productsQuery.AsNoTracking(), numpag ?? 1, regQuantity));
      
        }
       
        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Category"] = new SelectList(_context.Categories, "IdCategory", "CategoryName");
            ViewData["Location"] = new SelectList(_context.Locations, "IdLocation", "LocationName");
            ViewData["State"] = GetStateItems();

            return View();
        }

        [HttpPost]
        //Para asegurar de recibir la informacion de nuestro propio formulario
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductViewModel model, IFormFile Image)
        {
            ViewData["Category"] = new SelectList(_context.Categories, "IdCategory", "CategoryName");
            ViewData["Location"] = new SelectList(_context.Locations, "IdLocation", "LocationName");
            ViewData["State"] = GetStateItems();
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
            ViewData["State"] = GetStateItems(); 
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
            ViewData["State"] = GetStateItems();

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

        public IActionResult GeneratePdf()
        {
            // Obtener todos los datos de la tabla ChangeLog
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Location)
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
            <h1>Products Report</h1>
            <table>
                <thead>
                    <tr>
                        <th>ProductName</th>
                        <th>Quantity</th>
                        <th>CategoryName</th>
                        <th>LocationName</th>
                        <th>State</th>
                        <th>Description</th>
                        <th>CreationDate</th>
                        <th>LastModDate</th>
                    </tr>
                </thead>
                <tbody>");

            foreach (var product in products)
            {
                htmlContent.Append("<tr>")
                           .AppendFormat("<td>{0}</td>", product.ProductName)
                           .AppendFormat("<td>{0}</td>", product.Quantity)
                            .AppendFormat("<td>{0}</td>", product.Category.CategoryName)
                           .AppendFormat("<td>{0}</td>", product.Location.LocationName)
                           .AppendFormat("<td>{0}</td>", product.State)
                           .AppendFormat("<td>{0}</td>", product.Description)
                           .AppendFormat("<td>{0:yyyy-MM-dd HH:mm:ss}</td>", product.CreationDate)
                           .AppendFormat("<td>{0}</td>", product.LastModDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? "Whithout modifications")
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
