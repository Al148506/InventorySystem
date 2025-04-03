using DinkToPdf;
using DinkToPdf.Contracts;
using InventorySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

namespace InventorySystem.Controllers
{
    public class ProductController : Controller
    {
        private readonly IConverter _converter;
        private static Dictionary<int, Product> _products = new();
        private static Dictionary<int, Category> _categories = new();
        private static Dictionary<int, Location> _locations = new();
        private static int _nextProductId = 1;

        public ProductController(IConverter converter)
        {
            _converter = converter;
        }

        public IActionResult Index(string searchName, int? categoryId, int? locationId)
        {
            var productsQuery = _products.Values.AsQueryable();
            if (!string.IsNullOrEmpty(searchName))
                productsQuery = productsQuery.Where(p => p.ProductName.Contains(searchName));
            if (categoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.IdCategory == categoryId.Value);
            if (locationId.HasValue)
                productsQuery = productsQuery.Where(p => p.IdLocation == locationId.Value);

            ViewData["Category"] = new SelectList(_categories.Values, "IdCategory", "CategoryName", categoryId);
            ViewData["Location"] = new SelectList(_locations.Values, "IdLocation", "LocationName", locationId);
            return View(productsQuery.ToList());



            
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewData["Category"] = new SelectList(_categories.Values, "IdCategory", "CategoryName");
            ViewData["Location"] = new SelectList(_locations.Values, "IdLocation", "LocationName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product product)
        {
            product.IdProd = _nextProductId++;
            product.CreationDate = DateTime.Now;
            product.LastModDate = DateTime.Now;
            _products[product.IdProd] = product;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!_products.ContainsKey(id)) return NotFound();
            return View(_products[id]);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            if (!_products.ContainsKey(product.IdProd)) return NotFound();
            product.LastModDate = DateTime.Now;
            _products[product.IdProd] = product;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            if (_products.ContainsKey(id)) _products.Remove(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
