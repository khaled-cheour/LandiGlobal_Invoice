/**
 * GUIDE : FICHIERS CLÉS PAR FONCTIONNALITÉ
 * 
 * Ce guide montre où trouver chaque composant dans le code
 * et comment les données circulent.
 */

// ========================================
// 1. EXTRACTION ET VALIDATION
// ========================================

/**
 * Fichiers impliqués:
 * └─ Services/PDFExtractionService.cs (Extraction PDF)
 * └─ Services/InvoiceValidationService.cs (Validation)
 * └─ Controllers/InvoiceController.cs (Upload endpoint)
 * 
 * Flux:
 * User Upload → InvoiceController.Upload()
 *   ├─ PDFExtractionService.ExtractInvoiceDataAsync()
 *   │  └─ Retourne: InvoiceHistory + InvoiceHistoryProduct[]
 *   ├─ InvoiceValidationService.ValidateInvoiceAsync()
 *   │  └─ Retourne: ValidationResult { Success, Errors }
 *   └─ DbContext.InvoiceHistories.Add() → SaveChangesAsync()
 *      └─ Données stockées en DB
 */

// Classes impliquées:

// 1.1 DATA MODEL (Data/InvoiceHistory.cs)
public class InvoiceHistory
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; }
    public string CustomerName { get; set; }
    public string Email { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; }
    public string Status { get; set; }  // "Processed", "Draft", "Error"
    public DateTime CreatedAt { get; set; }
    public virtual ICollection<InvoiceHistoryProduct> Products { get; set; }
}

public class InvoiceHistoryProduct
{
    public int Id { get; set; }
    public int InvoiceHistoryId { get; set; }
    public string ProductNumber { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public virtual InvoiceHistory Invoice { get; set; }
}

// 1.2 DATABASE CONTEXT (Data/ApplicationDbContext.cs)
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<InvoiceHistory> InvoiceHistories { get; set; }
    public DbSet<InvoiceHistoryProduct> InvoiceHistoryProducts { get; set; }
    // Plus d'autres DbSets...
}

// 1.3 VALIDATION SERVICE (Services/InvoiceValidationService.cs)
public class InvoiceValidationService
{
    public async Task<ValidationResult> ValidateInvoiceAsync(InvoiceHistory invoice)
    {
        var errors = new List<string>();
        
        // Règle 1: Email
        if (!IsValidEmail(invoice.Email))
            errors.Add("Email invalide");
        
        // Règle 2: Montant
        if (invoice.TotalAmount <= 0)
            errors.Add("Montant invalide");
        
        // Règle 3: Unicité
        var exists = await _context.InvoiceHistories
            .AnyAsync(i => i.InvoiceNumber == invoice.InvoiceNumber);
        if (exists)
            errors.Add("Facture déjà existante");
        
        // ... plus de règles
        
        return new ValidationResult 
        { 
            Success = errors.Count == 0,
            Errors = errors 
        };
    }
}

// ========================================
// 2. NOTIFICATIONS EMAIL
// ========================================

/**
 * Fichiers impliqués:
 * └─ Services/EmailService.cs (SMTP & Templates)
 * └─ Controllers/InvoiceController.cs (Appel du service)
 * └─ Program.cs (Configuration SMTP)
 * 
 * Configuration:
 * appsettings.json:
 * {
 *   "Email": {
 *     "SmtpServer": "smtp.gmail.com",
 *     "SmtpPort": 587,
 *     "Username": "your-email@gmail.com",
 *     "Password": "your-app-password"
 *   }
 * }
 * 
 * Flux:
 * Facture créée
 *   └─ EmailService.SendInvoiceNotificationAsync(invoice)
 *      ├─ Construit HTML du mail
 *      ├─ Se connecte à SMTP
 *      └─ Envoie à invoice.Email
 * 
 * Facture validée
 *   └─ EmailService.SendValidationNotificationAsync()
 * 
 * Export réussi
 *   └─ EmailService.SendExportReportAsync(attachment)
 */

// Classes impliquées:

// 2.1 EMAIL SERVICE (Services/EmailService.cs)
public class EmailService
{
    private readonly IConfiguration _config;
    
    public async Task SendEmailAsync(
        string to, string subject, string body, bool isHtml = true)
    {
        using (var client = new SmtpClient(_config["Email:SmtpServer"]))
        {
            client.Port = int.Parse(_config["Email:SmtpPort"]);
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(
                _config["Email:Username"],
                _config["Email:Password"]
            );
            client.EnableSsl = true;
            
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Landi Global", _config["Email:Username"]));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;
            
            message.Body = new TextPart(isHtml ? "html" : "plain") { Text = body };
            
            await client.SendAsync(message);
        }
    }
    
    public async Task SendInvoiceNotificationAsync(InvoiceHistory invoice)
    {
        var body = $@"
            <h2>Invoice {invoice.InvoiceNumber}</h2>
            <p>Customer: {invoice.CustomerName}</p>
            <p>Amount: ${invoice.TotalAmount:F2}</p>
        ";
        
        await SendEmailAsync(invoice.Email, $"Invoice {invoice.InvoiceNumber}", body);
    }
}

// ========================================
// 3. DASHBOARD
// ========================================

/**
 * Fichiers impliqués:
 * └─ Controllers/DashboardController.cs (Logique)
 * └─ Views/Dashboard/Index.cshtml (UI)
 * 
 * Flux:
 * User visite /dashboard
 *   └─ DashboardController.Index()
 *      ├─ var invoices = _context.InvoiceHistories.ToListAsync()
 *      ├─ Calcule: TotalInvoices, TotalRevenue, AveragePerInvoice, etc.
 *      ├─ Crée DashboardViewModel avec les stats
 *      └─ return View(stats)
 * 
 * View affiche:
 *   ├─ 4 KPI Cards (nombre, revenu, moyenne, taux succès)
 *   ├─ Graphique Chart.js: Revenu mensuel
 *   ├─ Graphique Chart.js: Distribution statuts
 *   └─ Table: Top 5 clients
 */

// Classes impliquées:

// 3.1 CONTROLLER (Controllers/DashboardController.cs)
[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public async Task<IActionResult> Index()
    {
        var invoices = await _context.InvoiceHistories
            .AsNoTracking()
            .Where(i => i.Status == "Processed")
            .ToListAsync();
        
        var model = new DashboardViewModel
        {
            TotalInvoices = invoices.Count,
            TotalRevenue = invoices.Sum(i => i.TotalAmount),
            AverageInvoiceValue = invoices.Count > 0 
                ? invoices.Sum(i => i.TotalAmount) / invoices.Count 
                : 0,
            // ... plus de stats
        };
        
        return View(model);
    }
    
    [HttpGet("api/chart-data")]
    [Produces("application/json")]
    public async Task<IActionResult> GetChartData()
    {
        // Retourne JSON pour Chart.js
        var data = new
        {
            monthlyRevenue = await GetMonthlyRevenueData(),
            revenueByStatus = await GetRevenueByStatus(),
            topCustomers = await GetTopCustomers()
        };
        
        return Json(data);
    }
}

// 3.2 VIEW (Views/Dashboard/Index.cshtml)
@model DashboardViewModel

<div class="container-fluid">
    <h1>Dashboard</h1>
    
    <div class="row mb-4">
        <div class="col-md-3">
            <div class="card">
                <div class="card-body">
                    <h5>Total Factures</h5>
                    <h3>@Model.TotalInvoices</h3>
                </div>
            </div>
        </div>
        <!-- Plus de cards -->
    </div>
    
    <div class="row">
        <div class="col-lg-6">
            <div class="card">
                <div class="card-header">Revenu Mensuel</div>
                <div class="card-body">
                    <canvas id="revenueChart"></canvas>
                </div>
            </div>
        </div>
        <!-- Plus de graphiques -->
    </div>
</div>

@section Scripts {
    <script src="https://cdn.jsdelivr.net/npm/chart.js@3.9.1"></script>
    <script>
        // Chart.js initialization
        fetch('/dashboard/api/chart-data')
            .then(r => r.json())
            .then(data => {
                var ctx = document.getElementById('revenueChart').getContext('2d');
                new Chart(ctx, {
                    type: 'line',
                    data: {
                        labels: data.monthlyRevenue.map(d => d.month),
                        datasets: [{
                            label: 'Revenue',
                            data: data.monthlyRevenue.map(d => d.revenue)
                        }]
                    }
                });
            });
    </script>
}

// ========================================
// 4. ANALYTICS
// ========================================

/**
 * Fichiers impliqués:
 * └─ Controllers/AnalyticsController.cs (Logique)
 * └─ Views/Analytics/Index.cshtml (UI)
 * 
 * Flux:
 * User visite /analytics
 *   └─ AnalyticsController.Index()
 *      └─ Retourne View (page vide avec scripts)
 * 
 * Page chargée
 *   └─ JavaScript appelle /analytics/api/chart-data
 *      ├─ GetMonthlyRevenueData() - Requête GROUP BY Month
 *      ├─ GetRevenueByStatus() - Requête GROUP BY Status
 *      ├─ GetTopCustomers() - Requête GROUP BY CustomerName, ORDER BY Revenue
 *      └─ Retourne JSON
 * 
 * Chart.js rend les graphiques
 */

// 4.1 CONTROLLER (Controllers/AnalyticsController.cs)
[Authorize]
public class AnalyticsController : Controller
{
    [HttpGet("api/chart-data")]
    [Produces("application/json")]
    public async Task<IActionResult> GetChartData()
    {
        var data = new
        {
            monthlyRevenue = await GetMonthlyRevenueData(),
            revenueByStatus = await GetRevenueByStatus(),
            topCustomers = await GetTopCustomersByRevenue(),
            conversionMetrics = await GetConversionMetrics()
        };
        
        return Json(data);
    }
    
    private async Task<object> GetMonthlyRevenueData()
    {
        var last12Months = Enumerable.Range(0, 12)
            .Select(i => DateTime.Now.AddMonths(-i))
            .OrderBy(d => d);
        
        var data = await Task.WhenAll(last12Months.Select(async month =>
        {
            var revenue = await _context.InvoiceHistories
                .Where(i => i.CreatedAt.Year == month.Year 
                         && i.CreatedAt.Month == month.Month)
                .SumAsync(i => i.TotalAmount);
            
            return new
            {
                month = month.ToString("MMM yyyy"),
                revenue = revenue
            };
        }));
        
        return data;
    }
}

// ========================================
// 5. RECHERCHE AVANCÉE
// ========================================

/**
 * Fichiers impliqués:
 * └─ Controllers/SearchController.cs (Logique)
 * └─ Views/Search/Advanced.cshtml (UI)
 * 
 * Flux:
 * User visite /search/advanced
 *   └─ SearchController.Advanced(filters...)
 *      ├─ Construit LINQ query
 *      ├─ Applique filtres: WHERE InvoiceNumber, CustomerName, Status, etc.
 *      ├─ Ajoute pagination: SKIP + TAKE
 *      └─ return View(results)
 * 
 * View affiche:
 *   ├─ Formulaire avec filtres (input, select)
 *   ├─ Table des résultats paginée
 *   └─ Contrôles de pagination (Previous, 1, 2, 3, Next)
 */

// 5.1 CONTROLLER (Controllers/SearchController.cs)
[Authorize]
public class SearchController : Controller
{
    public async Task<IActionResult> Advanced(
        string invoiceNumber,
        string customerName,
        string status,
        decimal? minAmount,
        decimal? maxAmount,
        int page = 1,
        int pageSize = 20)
    {
        var query = _context.InvoiceHistories.AsNoTracking();
        
        if (!string.IsNullOrEmpty(invoiceNumber))
            query = query.Where(i => i.InvoiceNumber.Contains(invoiceNumber));
        
        if (!string.IsNullOrEmpty(customerName))
            query = query.Where(i => i.CustomerName.Contains(customerName));
        
        if (!string.IsNullOrEmpty(status))
            query = query.Where(i => i.Status == status);
        
        if (minAmount.HasValue)
            query = query.Where(i => i.TotalAmount >= minAmount);
        
        if (maxAmount.HasValue)
            query = query.Where(i => i.TotalAmount <= maxAmount);
        
        var totalCount = await query.CountAsync();
        var skip = (page - 1) * pageSize;
        
        var results = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
        
        ViewBag.TotalCount = totalCount;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        return View(results);
    }
}

// 5.2 VIEW (Views/Search/Advanced.cshtml)
@model List<InvoiceHistory>

<div class="container">
    <h1>Recherche Avancée</h1>
    
    <form method="get" class="row g-3">
        <div class="col-md-2">
            <label>N° Facture</label>
            <input type="text" name="invoiceNumber" class="form-control" />
        </div>
        <div class="col-md-2">
            <label>Client</label>
            <input type="text" name="customerName" class="form-control" />
        </div>
        <div class="col-md-2">
            <label>Montant Min</label>
            <input type="number" name="minAmount" class="form-control" step="0.01" />
        </div>
        <!-- Plus de champs -->
        <button type="submit" class="btn btn-primary">Rechercher</button>
    </form>
    
    <table class="table">
        <thead>
            <tr>
                <th>N° Facture</th>
                <th>Client</th>
                <th>Montant</th>
                <th>Statut</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var invoice in Model)
            {
                <tr>
                    <td>@invoice.InvoiceNumber</td>
                    <td>@invoice.CustomerName</td>
                    <td>$@invoice.TotalAmount</td>
                    <td>@invoice.Status</td>
                </tr>
            }
        </tbody>
    </table>
    
    <!-- Pagination controls -->
</div>

// ========================================
// 6. EXPORT EXCEL/CSV
// ========================================

/**
 * Fichiers impliqués:
 * └─ Controllers/ExportController.cs (API endpoints)
 * └─ Services/ExportService.cs (Logique d'export)
 * 
 * Flux:
 * User clique "Export Excel"
 *   └─ GET /api/export/invoices/excel
 *      ├─ ExportService.ExportInvoicesToExcelAsync()
 *      │  ├─ Crée workbook ClosedXML
 *      │  ├─ Ajoute 3 feuilles: Factures, Produits, Résumé
 *      │  ├─ Format les colonnes (numbers, colors)
 *      │  └─ Retourne MemoryStream
 *      └─ Retourne FileStream au user
 *         └─ Navigateur télécharge le fichier .xlsx
 */

// 6.1 API CONTROLLER (Controllers/ExportController.cs)
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly ExportService _exportService;
    
    [HttpGet("invoices/excel")]
    public async Task<IActionResult> ExportInvoicesToExcel()
    {
        var stream = await _exportService.ExportInvoicesToExcelAsync();
        
        return File(
            stream,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "invoices.xlsx"
        );
    }
}

// 6.2 EXPORT SERVICE (Services/ExportService.cs)
public class ExportService
{
    public async Task<MemoryStream> ExportInvoicesToExcelAsync()
    {
        var invoices = await _context.InvoiceHistories
            .Include(i => i.Products)
            .ToListAsync();
        
        var workbook = new XLWorkbook();
        var wsInvoices = workbook.Worksheets.Add("Factures");
        
        // En-têtes
        wsInvoices.Cell(1, 1).Value = "Invoice N°";
        wsInvoices.Cell(1, 2).Value = "Customer";
        wsInvoices.Cell(1, 3).Value = "Amount";
        // ... plus de colonnes
        
        // Données
        int row = 2;
        foreach (var invoice in invoices)
        {
            wsInvoices.Cell(row, 1).Value = invoice.InvoiceNumber;
            wsInvoices.Cell(row, 2).Value = invoice.CustomerName;
            wsInvoices.Cell(row, 3).Value = invoice.TotalAmount;
            row++;
        }
        
        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;
        
        return stream;
    }
}

// ========================================
// 7. BATCH UPLOAD (À IMPLÉMENTER)
// ========================================

/**
 * Fichiers à créer:
 * └─ Controllers/BatchUploadController.cs (Endpoint upload)
 * └─ Services/BatchUploadService.cs (Logique batch)
 * 
 * Flux:
 * User upload Excel (150 factures)
 *   └─ POST /api/batch-upload/invoices
 *      ├─ Parse le fichier
 *      ├─ Boucle: FOR EACH invoice
 *      │  └─ ValidateInvoiceAsync(invoice)
 *      ├─ Sépare: Valides vs Invalides
 *      ├─ INSERT les valides en DB
 *      ├─ Crée rapport
 *      └─ Envoie email du rapport
 *         └─ "145 factures importées, 5 erreurs"
 */

// 7.1 API CONTROLLER (À créer)
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class BatchUploadController : ControllerBase
{
    private readonly BatchUploadService _batchService;
    
    [HttpPost("invoices")]
    public async Task<IActionResult> UploadInvoiceBatch(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Fichier vide");
        
        var result = await _batchService.ProcessBatchUploadAsync(file);
        
        return Ok(result);
        // Exemple de résponse:
        // {
        //   "batchId": "BATCH-2026-04-22-001",
        //   "totalCount": 150,
        //   "successCount": 145,
        //   "errorCount": 5,
        //   "successRate": 96.7,
        //   "errors": [...]
        // }
    }
}

// ========================================
// RÉSUMÉ DES ROUTES
// ========================================

/*
GET  /                              → Home page
GET  /account/register              → Registration form
GET  /account/login                 → Login form
POST /account/login                 → Login endpoint
GET  /dashboard                     → Dashboard
GET  /dashboard/api/chart-data      → Dashboard API data
GET  /analytics                     → Analytics page
GET  /analytics/api/chart-data      → Analytics API data
GET  /search/advanced               → Advanced search page
POST /api/search                    → Search API endpoint
GET  /api/export/invoices/excel     → Export to Excel
GET  /api/export/invoices/csv       → Export to CSV
POST /api/batch-upload/invoices     → Batch upload endpoint (À implémenter)
GET  /swagger                       → API documentation
*/
