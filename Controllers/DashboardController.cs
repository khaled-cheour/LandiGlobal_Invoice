using LandiGlobalTemplate.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LandiGlobalTemplate.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(ApplicationDbContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            if (Request.Path.HasValue)
            {
                return RedirectToAction("Index", "Invoice");
            }

            try
            {
                var today = DateTime.UtcNow.Date;
                var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

                // Get invoices from database
                var allInvoices = await _context.InvoiceHistories.AsNoTracking().ToListAsync();
                var todayInvoices = allInvoices.Where(x => x.CreatedAt.Date == today).ToList();
                var monthInvoices = allInvoices.Where(x => x.CreatedAt >= startOfMonth && x.CreatedAt.Date <= today).ToList();

                // Statistics
                var dashboard = new DashboardViewModel
                {
                    TotalInvoices = allInvoices.Count,
                    TotalRevenue = allInvoices.Sum(x => x.TotalAmount),
                    ProcessedCount = allInvoices.Count(x => x.Status == "Processed"),
                    DraftCount = allInvoices.Count(x => x.Status == "Draft"),
                    ErrorCount = allInvoices.Count(x => x.Status == "Error"),
                    AverageInvoiceValue = allInvoices.Count > 0 ? allInvoices.Average(x => x.TotalAmount) : 0,
                    TodayInvoices = todayInvoices.Count,
                    TodayRevenue = todayInvoices.Sum(x => x.TotalAmount),
                    CurrentMonthInvoices = monthInvoices.Count,
                    CurrentMonthRevenue = monthInvoices.Sum(x => x.TotalAmount),
                    
                    // Top customers
                    TopCustomers = allInvoices
                        .GroupBy(x => x.CustomerName)
                        .OrderByDescending(g => g.Count())
                        .Take(5)
                        .Select(g => new CustomerSalesDto
                        {
                            Name = g.Key,
                            Count = g.Count(),
                            TotalAmount = g.Sum(x => x.TotalAmount)
                        })
                        .ToList(),

                    // Monthly trend
                    MonthlySales = Enumerable.Range(0, 12)
                        .Select(i =>
                        {
                            var month = DateTime.UtcNow.AddMonths(-11 + i);
                            var monthStart = new DateTime(month.Year, month.Month, 1);
                            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                            
                            var monthData = allInvoices.Where(x =>
                                x.CreatedAt >= monthStart && x.CreatedAt <= monthEnd.AddDays(1)).ToList();
                            
                            return new MonthlySalesDto
                            {
                                Month = monthStart.ToString("MMM yyyy"),
                                Amount = monthData.Sum(x => x.TotalAmount),
                                Count = monthData.Count
                            };
                        })
                        .ToList(),

                    // Status distribution
                    StatusDistribution = new Dictionary<string, int>
                    {
                        { "Processed", allInvoices.Count(x => x.Status == "Processed") },
                        { "Draft", allInvoices.Count(x => x.Status == "Draft") },
                        { "Error", allInvoices.Count(x => x.Status == "Error") }
                    },

                    // Recent invoices
                    RecentInvoices = allInvoices
                        .OrderByDescending(x => x.CreatedAt)
                        .Take(10)
                        .Select(x => new RecentInvoiceDto
                        {
                            Id = x.Id,
                            InvoiceNumber = x.InvoiceNumber,
                            CustomerName = x.CustomerName,
                            Amount = x.TotalAmount,
                            Status = x.Status,
                            CreatedAt = x.CreatedAt,
                            IsValidated = x.IsValidated
                        })
                        .ToList()
                };

                return View(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du chargement du dashboard");
                return View(new DashboardViewModel());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetChartData()
        {
            try
            {
                var invoices = await _context.InvoiceHistories.AsNoTracking().ToListAsync();

                var monthlyData = Enumerable.Range(0, 12)
                    .Select(i =>
                    {
                        var month = DateTime.UtcNow.AddMonths(-11 + i);
                        var monthStart = new DateTime(month.Year, month.Month, 1);
                        var monthEnd = monthStart.AddMonths(1);
                        
                        var monthInvoices = invoices.Where(x =>
                            x.CreatedAt >= monthStart && x.CreatedAt < monthEnd).ToList();
                        
                        return new
                        {
                            month = monthStart.ToString("MMM"),
                            revenue = monthInvoices.Sum(x => x.TotalAmount),
                            count = monthInvoices.Count
                        };
                    })
                    .ToList();

                var statusData = new[]
                {
                    new { status = "Processed", count = invoices.Count(x => x.Status == "Processed") },
                    new { status = "Draft", count = invoices.Count(x => x.Status == "Draft") },
                    new { status = "Error", count = invoices.Count(x => x.Status == "Error") }
                };

                return Json(new
                {
                    monthly = monthlyData,
                    status = statusData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des données du graphique");
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoices(int page = 1, int pageSize = 10, string? search = null, string? status = null)
        {
            try
            {
                var query = _context.InvoiceHistories.AsNoTracking().AsQueryable();

                // Search filter
                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(x =>
                        x.InvoiceNumber.Contains(search) ||
                        x.CustomerName.Contains(search) ||
                        x.Email.Contains(search));
                }

                // Status filter
                if (!string.IsNullOrWhiteSpace(status) && status != "All")
                {
                    query = query.Where(x => x.Status == status);
                }

                var total = await query.CountAsync();
                var invoices = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new RecentInvoiceDto
                    {
                        Id = x.Id,
                        InvoiceNumber = x.InvoiceNumber,
                        CustomerName = x.CustomerName,
                        Amount = x.TotalAmount,
                        Status = x.Status,
                        CreatedAt = x.CreatedAt,
                        IsValidated = x.IsValidated
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    total = total,
                    page = page,
                    pageSize = pageSize,
                    data = invoices
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des factures");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoiceDetail(int id)
        {
            try
            {
                var invoice = await _context.InvoiceHistories
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id);

                if (invoice == null)
                {
                    return Json(new { success = false, message = "Facture non trouvée" });
                }

                return Json(new
                {
                    success = true,
                    invoice = new
                    {
                        id = invoice.Id,
                        invoiceNumber = invoice.InvoiceNumber,
                        customerName = invoice.CustomerName,
                        email = invoice.Email,
                        phone = invoice.Phone,
                        totalAmount = invoice.TotalAmount,
                        currency = invoice.Currency,
                        invoiceDate = invoice.InvoiceDate,
                        billingAddress = invoice.BillingAddress,
                        shipToAddress = invoice.ShipToAddress,
                        status = invoice.Status,
                        isValidated = invoice.IsValidated,
                        productCount = invoice.ProductCount,
                        errorMessage = invoice.ErrorMessage
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la récupération des détails de la facture");
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    // View Models
    public class DashboardViewModel
    {
        public int TotalInvoices { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ProcessedCount { get; set; }
        public int DraftCount { get; set; }
        public int ErrorCount { get; set; }
        public decimal AverageInvoiceValue { get; set; }
        
        public int TodayInvoices { get; set; }
        public decimal TodayRevenue { get; set; }
        public int CurrentMonthInvoices { get; set; }
        public decimal CurrentMonthRevenue { get; set; }

        public List<CustomerSalesDto> TopCustomers { get; set; } = new();
        public List<MonthlySalesDto> MonthlySales { get; set; } = new();
        public Dictionary<string, int> StatusDistribution { get; set; } = new();
        public List<RecentInvoiceDto> RecentInvoices { get; set; } = new();
    }

    public class CustomerSalesDto
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class MonthlySalesDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int Count { get; set; }
    }

    public class RecentInvoiceDto
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsValidated { get; set; }
    }
}
