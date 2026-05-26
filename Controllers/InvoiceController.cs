using LandiGlobalTemplate.Models;
using LandiGlobalTemplate.Services;
using LandiGlobalTemplate.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static LandiGlobalTemplate.Services.InvoiceValidationService;

namespace LandiGlobalTemplate.Controllers
{
    public class InvoiceController : Controller
    {
        private readonly PDFExtractionService _extractionService;
        private readonly PDFGenerationService _generationService;
        private readonly ApplicationDbContext _context;
        private readonly InvoiceValidationService _validationService;

        public InvoiceController(ApplicationDbContext context, InvoiceValidationService validationService)
        {
            _extractionService = new PDFExtractionService();
            _generationService = new PDFGenerationService();
            _context = context;
            _validationService = validationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file");
                return View("Index");
            }

            if (file.ContentType != "application/pdf")
            {
                ModelState.AddModelError("", "Please upload a PDF file");
                return View("Index");
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    var pdfBytes = memoryStream.ToArray();

                    // Extract data from PDF
                    var invoice = _extractionService.ExtractFromPDF(pdfBytes);
                    await EnrichInvoiceProductsFromDatabaseAsync(invoice);
                    await ValidateExtractedProductsAreKnownAsync(invoice);

                    if (invoice.ExtractionErrors.Count > 0)
                    {
                        foreach (var error in invoice.ExtractionErrors)
                        {
                            ModelState.AddModelError("", error);
                        }

                        return View("Index");
                    }

                    // Store invoice in session for display and download
                    HttpContext.Session.SetString("InvoiceData", System.Text.Json.JsonSerializer.Serialize(invoice));

                    return RedirectToAction("Display");
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error processing PDF: {ex.Message}");
                return View("Index");
            }
        }

        public IActionResult Display()
        {
            var invoiceJson = HttpContext.Session.GetString("InvoiceData");
            if (string.IsNullOrEmpty(invoiceJson))
            {
                return RedirectToAction("Index");
            }

            var invoice = System.Text.Json.JsonSerializer.Deserialize<CommercialInvoice>(invoiceJson);
            return View(invoice);
        }

        [HttpPost]
        public IActionResult UpdateProduct(string productNumber, string familyName, string description, 
            string hsCode, string memory, string fourG, string gms, string paymentType, string color, string size)
        {
            var invoiceJson = HttpContext.Session.GetString("InvoiceData");
            if (string.IsNullOrEmpty(invoiceJson))
            {
                return Json(new { success = false, message = "Invoice not found" });
            }

            var invoice = System.Text.Json.JsonSerializer.Deserialize<CommercialInvoice>(invoiceJson);
            if (invoice == null)
            {
                return Json(new { success = false, message = "Failed to parse invoice" });
            }

            var product = invoice.Products.FirstOrDefault(p => p.ProductNumber == productNumber);
            if (product != null)
            {
                product.FamilyName = familyName;
                product.ProductDescription = description;
                product.HSCode = hsCode;
                product.MemoryPlan = memory;
                product.G4 = fourG;
                product.GMS = gms;
                product.PaymentType = paymentType;
                product.Color = color;
                product.Size = size;

                HttpContext.Session.SetString("InvoiceData", System.Text.Json.JsonSerializer.Serialize(invoice));
                return Json(new { success = true, message = "Product updated successfully" });
            }

            return Json(new { success = false, message = "Product not found" });
        }

        public IActionResult DownloadPDF()
        {
            var invoiceJson = HttpContext.Session.GetString("InvoiceData");
            if (string.IsNullOrEmpty(invoiceJson))
            {
                return RedirectToAction("Index");
            }

            var invoice = System.Text.Json.JsonSerializer.Deserialize<CommercialInvoice>(invoiceJson);
            if (invoice == null || invoice.InvoiceInfo == null)
            {
                return RedirectToAction("Index");
            }

            var pdfBytes = _generationService.GenerateInvoicePDF(invoice);
            var fileName = $"Invoice_{invoice.InvoiceInfo.InvoiceNumber}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }

        /// <summary>
        /// Download a saved invoice from the database by its ID
        /// </summary>
        public async Task<IActionResult> Download(int id)
        {
            try
            {
                // Récupérer la facture de la base de données avec ses produits
                var invoiceHistory = await _context.InvoiceHistories
                    .Include(i => i.Products)
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (invoiceHistory == null)
                {
                    return NotFound("Invoice not found");
                }

                // Convertir InvoiceHistory en CommercialInvoice pour la génération PDF
                var commercialInvoice = new CommercialInvoice
                {
                    InvoiceInfo = new InvoiceInformation
                    {
                        InvoiceNumber = invoiceHistory.InvoiceNumber,
                        InvoiceDate = invoiceHistory.InvoiceDate.ToString("yyyy-MM-dd"),
                        PurchaseOrderNumber = invoiceHistory.PurchaseOrderNumber,
                        PaymentTerm = invoiceHistory.PaymentTerm,
                        Incoterms = invoiceHistory.Incoterms,
                        CountryOfOrigin = invoiceHistory.CountryOfOrigin
                    },
                    CustomerInfo = new CustomerInformation
                    {
                        CustomerName = invoiceHistory.CustomerName,
                        Email = invoiceHistory.Email,
                        Phone = invoiceHistory.Phone,
                        BillingAddress = invoiceHistory.BillingAddress,
                        ShipToAddress = invoiceHistory.ShipToAddress,
                        ShipFromAddress = invoiceHistory.ShipFromAddress,
                        VAT = invoiceHistory.VAT,
                        EORI = invoiceHistory.EORI
                    },
                    Products = invoiceHistory.Products.Select(p => new ProductInformation
                    {
                        ProductNumber = p.ProductNumber,
                        FamilyName = p.FamilyName,
                        ProductDescription = p.ProductDescription,
                        Platform = p.Platform,
                        Model = p.Model,
                        MainDisplay = p.MainDisplay,
                        SecondDisplay = p.SecondDisplay,
                        Quantity = p.Quantity,
                        UnitPrice = p.UnitPrice,
                        TotalAmount = p.TotalAmount,
                        HSCode = p.HSCode,
                        PaymentType = p.PaymentType,
                        MemoryPlan = p.MemoryPlan,
                        G4 = p.G4,
                        GMS = p.GMS,
                        Color = p.Color,
                        Size = p.Size
                    }).ToList(),
                    TotalAmount = invoiceHistory.TotalAmount,
                    Currency = invoiceHistory.Currency
                };

                // Générer le PDF
                var pdfBytes = _generationService.GenerateInvoicePDF(commercialInvoice);
                var fileName = $"Invoice_{invoiceHistory.InvoiceNumber}_{invoiceHistory.InvoiceDate:yyyyMMdd}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating invoice PDF: {ex.Message}");
            }
        }

        public IActionResult Clear()
        {
            HttpContext.Session.Remove("InvoiceData");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SaveToDatabase()
        {
            try
            {
                // Récupérer les données de la session
                var invoiceJson = HttpContext.Session.GetString("InvoiceData");
                if (string.IsNullOrEmpty(invoiceJson))
                {
                    return Json(new { success = false, message = "No invoice data found" });
                }

                var extractedInvoice = System.Text.Json.JsonSerializer.Deserialize<CommercialInvoice>(invoiceJson);
                if (extractedInvoice == null)
                {
                    return Json(new { success = false, message = "Failed to parse invoice data" });
                }

                // Parser la date depuis string
                DateTime invoiceDate = DateTime.Now;
                if (extractedInvoice.InvoiceInfo != null && !string.IsNullOrEmpty(extractedInvoice.InvoiceInfo.InvoiceDate))
                {
                    if (DateTime.TryParse(extractedInvoice.InvoiceInfo.InvoiceDate, out var parsedDate))
                    {
                        invoiceDate = parsedDate;
                    }
                }

                // Créer l'objet InvoiceHistory pour la BD
                var invoiceHistory = new Data.InvoiceHistory
                {
                    InvoiceNumber = extractedInvoice.InvoiceInfo?.InvoiceNumber ?? "UNKNOWN",
                    InvoiceDate = invoiceDate,
                    CustomerName = extractedInvoice.CustomerInfo?.CustomerName ?? "Unknown",
                    Email = extractedInvoice.CustomerInfo?.Email ?? "",
                    Phone = extractedInvoice.CustomerInfo?.Phone ?? "",
                    BillingAddress = extractedInvoice.CustomerInfo?.BillingAddress ?? "",
                    ShipToAddress = extractedInvoice.CustomerInfo?.ShipToAddress ?? "",
                    ShipFromAddress = extractedInvoice.CustomerInfo?.ShipFromAddress ?? "",
                    VAT = extractedInvoice.CustomerInfo?.VAT ?? "",
                    EORI = extractedInvoice.CustomerInfo?.EORI ?? "",
                    PaymentTerm = extractedInvoice.InvoiceInfo?.PaymentTerm ?? "",
                    Incoterms = extractedInvoice.InvoiceInfo?.Incoterms ?? "",
                    CountryOfOrigin = extractedInvoice.InvoiceInfo?.CountryOfOrigin ?? "",
                    TotalAmount = extractedInvoice.TotalAmount,
                    Currency = extractedInvoice.Currency ?? "USD",
                    ProductCount = extractedInvoice.Products?.Count ?? 0,
                    CreatedAt = DateTime.Now,
                    Status = "Draft",  // Initial status
                    IsValidated = false,
                    IsEnriched = false,
                    Products = new List<Data.InvoiceHistoryProduct>()
                };

                // Convertir et ajouter les produits
                if (extractedInvoice.Products != null && extractedInvoice.Products.Count > 0)
                {
                    foreach (var product in extractedInvoice.Products)
                    {
                        invoiceHistory.Products.Add(new Data.InvoiceHistoryProduct
                        {
                            ProductNumber = product.ProductNumber ?? "UNKNOWN",
                            FamilyName = product.FamilyName ?? "",
                            ProductDescription = product.ProductDescription ?? "",
                            Platform = product.Platform ?? "",
                            Model = product.Model ?? "",
                            MainDisplay = product.MainDisplay ?? "",
                            SecondDisplay = product.SecondDisplay ?? "",
                            Quantity = product.Quantity > 0 ? product.Quantity : 1,
                            UnitPrice = product.UnitPrice > 0 ? product.UnitPrice : 0,
                            TotalAmount = product.TotalAmount > 0 ? product.TotalAmount : 0,
                            HSCode = product.HSCode ?? "",
                            PaymentType = product.PaymentType ?? "",
                            MemoryPlan = product.MemoryPlan ?? "",
                            G4 = product.G4 ?? "",
                            GMS = product.GMS ?? "",
                            Color = product.Color ?? "",
                            Size = product.Size ?? "",
                            IsValid = false,
                            IsEnriched = false
                        });
                    }
                }

                // Valider la facture
                var validationResult = await _validationService.ValidateInvoiceAsync(invoiceHistory);

                if (validationResult.IsValid)
                {
                    invoiceHistory.Status = "Processed";
                    invoiceHistory.IsValidated = true;
                }
                else
                {
                    invoiceHistory.Status = "Error";
                    invoiceHistory.ErrorMessage = string.Join("; ", validationResult.Errors);
                }

                // Ajouter à la base de données
                _context.InvoiceHistories.Add(invoiceHistory);
                await _context.SaveChangesAsync();

                // Vider la session
                HttpContext.Session.Remove("InvoiceData");

                return Json(new 
                { 
                    success = true, 
                    message = validationResult.IsValid 
                        ? "Invoice saved successfully to database" 
                        : "Invoice saved with validation errors",
                    invoiceId = invoiceHistory.Id,
                    status = invoiceHistory.Status,
                    errors = validationResult.Errors
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error saving invoice: {ex.Message}" });
            }
        }

        /// <summary>
        /// API endpoint to validate if a product number is valid in the catalog
        /// </summary>
        [HttpGet]
        [Route("api/product/validate/{productNumber}")]
        public async Task<IActionResult> ValidateProduct(string productNumber)
        {
            var product = await GetProductFromDatabaseAsync(productNumber);
            var catalogProduct = product == null ? ProductCatalog.GetProduct(productNumber) : null;
            var isValid = product != null || ProductCatalog.IsValidProductNumber(productNumber);

            return Json(new
            {
                isValid = isValid,
                productNumber = productNumber,
                product = product != null ? new
                {
                    familyName = product.FamilyName,
                    platform = product.Platform,
                    model = product.Model,
                    mainDisplay = product.MainDisplay,
                    secondDisplay = product.SecondDisplay,
                    paymentType = product.PaymentType,
                    memoryPlan = product.MemoryPlan,
                    g4 = product.G4,
                    gms = product.GMS,
                    hsCode = product.HSCode
                } : catalogProduct != null ? new
                {
                    familyName = catalogProduct.FamilyName,
                    platform = catalogProduct.Platform,
                    model = catalogProduct.Model,
                    mainDisplay = catalogProduct.MainDisplay,
                    secondDisplay = catalogProduct.SecondDisplay,
                    paymentType = catalogProduct.PaymentType,
                    memoryPlan = catalogProduct.MemoryPlan,
                    g4 = catalogProduct.G4,
                    gms = catalogProduct.GMS,
                    hsCode = catalogProduct.HSCode
                } : null
            });
        }

        /// <summary>
        /// API endpoint to enrich product information from catalog
        /// </summary>
        [HttpPost]
        [Route("api/product/enrich")]
        public async Task<IActionResult> EnrichProduct([FromBody] ProductInformation product)
        {
            if (product == null || string.IsNullOrEmpty(product.ProductNumber))
            {
                return Json(new { success = false, message = "Product number is required" });
            }

            var databaseProduct = await GetProductFromDatabaseAsync(product.ProductNumber);
            if (databaseProduct != null)
            {
                ApplyProductData(product, databaseProduct);
            }
            else
            {
                ProductCatalog.EnrichProductInfo(product);
            }

            return Json(new
            {
                success = true,
                product = product
            });
        }

        private async Task EnrichInvoiceProductsFromDatabaseAsync(CommercialInvoice invoice)
        {
            if (invoice.Products == null || invoice.Products.Count == 0)
            {
                return;
            }

            foreach (var product in invoice.Products)
            {
                var databaseProduct = await GetProductFromDatabaseAsync(product.ProductNumber);
                if (databaseProduct != null)
                {
                    ApplyProductData(product, databaseProduct);
                }
            }
        }

        private async Task ValidateExtractedProductsAreKnownAsync(CommercialInvoice invoice)
        {
            if (invoice.Products == null || invoice.Products.Count == 0)
            {
                return;
            }

            foreach (var product in invoice.Products)
            {
                if (ProductCatalog.IsValidProductNumber(product.ProductNumber))
                {
                    continue;
                }

                var databaseProduct = await GetProductFromDatabaseAsync(product.ProductNumber);
                if (databaseProduct != null)
                {
                    continue;
                }

                invoice.ExtractionErrors.Add(
                    $"Il y a un problème sur votre facture : le code produit '{product.ProductNumber}' n'existe pas dans la base de données.");
            }
        }

        private async Task<ProductCatalogItem?> GetProductFromDatabaseAsync(string productNumber)
        {
            if (string.IsNullOrWhiteSpace(productNumber))
            {
                return null;
            }

            await ProductCatalogDatabaseInitializer.EnsureReadyAsync(_context);

            var normalizedProductNumber = productNumber.Trim();
            return await _context.ProductCatalogItems
                .AsNoTracking()
                .FirstOrDefaultAsync(product => product.ProductNumber == normalizedProductNumber);
        }

        private static void ApplyProductData(ProductInformation productInfo, ProductCatalogItem product)
        {
            productInfo.FamilyName = product.FamilyName;
            productInfo.Platform = product.Platform;
            productInfo.Model = product.Model;
            productInfo.ProductDescription = product.Model;
            productInfo.MainDisplay = product.MainDisplay;
            productInfo.SecondDisplay = product.SecondDisplay;
            productInfo.PaymentType = product.PaymentType;
            productInfo.MemoryPlan = product.MemoryPlan;
            productInfo.G4 = product.G4;
            productInfo.GMS = product.GMS;
            productInfo.HSCode = product.HSCode ?? string.Empty;
        }
    }
}
