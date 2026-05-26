using LandiGlobalTemplate.Data;
using LandiGlobalTemplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LandiGlobalTemplate.Controllers
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ApplicationDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            await ProductCatalogDatabaseInitializer.EnsureReadyAsync(_context);
            await EnsureRequestedProductExistsAsync();

            var products = await _context.ProductCatalogItems
                .AsNoTracking()
                .OrderBy(product => product.ProductNumber)
                .ToListAsync();

            return View(products);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCatalogItem product)
        {
            await ProductCatalogDatabaseInitializer.EnsureReadyAsync(_context);

            product.ProductNumber = product.ProductNumber?.Trim() ?? string.Empty;
            product.Model = product.Model?.Trim() ?? string.Empty;
            product.FamilyName = product.FamilyName?.Trim() ?? string.Empty;
            product.Platform = product.Platform?.Trim() ?? string.Empty;
            product.MainDisplay = product.MainDisplay?.Trim() ?? string.Empty;
            product.SecondDisplay = product.SecondDisplay?.Trim() ?? string.Empty;
            product.PaymentType = product.PaymentType?.Trim() ?? string.Empty;
            product.MemoryPlan = product.MemoryPlan?.Trim() ?? string.Empty;
            product.G4 = product.G4?.Trim() ?? string.Empty;
            product.GMS = product.GMS?.Trim() ?? string.Empty;
            product.HSCode = string.IsNullOrWhiteSpace(product.HSCode) ? null : product.HSCode.Trim();

            if (await _context.ProductCatalogItems.AnyAsync(existing => existing.ProductNumber == product.ProductNumber))
            {
                ModelState.AddModelError(nameof(product.ProductNumber), "Ce numéro de produit existe déjà.");
            }

            if (!ModelState.IsValid)
            {
                var products = await _context.ProductCatalogItems
                    .AsNoTracking()
                    .OrderBy(item => item.ProductNumber)
                    .ToListAsync();

                ViewData["ProductDraft"] = product;
                return View("Index", products);
            }

            product.CreatedAt = DateTime.UtcNow;
            _context.ProductCatalogItems.Add(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Produit {product.ProductNumber} ajouté.";
            return RedirectToAction(nameof(Index));
        }

        private async Task EnsureRequestedProductExistsAsync()
        {
            const string productNumber = "WX01000012";

            if (await _context.ProductCatalogItems.AnyAsync(product => product.ProductNumber == productNumber))
            {
                return;
            }

            _context.ProductCatalogItems.Add(new ProductCatalogItem
            {
                ProductNumber = productNumber,
                FamilyName = "Accessory",
                Platform = "Accessory",
                Model = "ECRPowerCordUK",
                MainDisplay = "-",
                SecondDisplay = "-",
                PaymentType = "-",
                MemoryPlan = "-",
                G4 = "-",
                GMS = "-",
                HSCode = null,
                CreatedAt = DateTime.UtcNow
            });

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogWarning(ex, "Le produit initial {ProductNumber} existe peut-être déjà.", productNumber);
            }
        }
    }
}
