using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LandiGlobalTemplate.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LandiGlobalTemplate.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Page principale des analyses avancées
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var model = await GetAnalyticsData();
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erreur lors du chargement des analyses: {ex.Message}";
                return RedirectToAction("Index", "Invoice");
            }
        }

        /// <summary>
        /// API endpoint pour les données des graphiques
        /// </summary>
        [HttpGet("api/chart-data")]
        [Produces("application/json")]
        public async Task<IActionResult> GetChartData()
        {
            try
            {
                var data = await Task.FromResult(new
                {
                    monthlyRevenue = await GetMonthlyRevenueData(),
                    revenueByStatus = await GetRevenueByStatus(),
                    revenueByCustomer = await GetTopCustomersByRevenue(),
                    invoicesByMonth = await GetInvoicesByMonth(),
                    averageInvoiceValue = await GetAverageInvoiceValue(),
                    totalTransactions = await GetTotalTransactions(),
                    conversionMetrics = await GetConversionMetrics()
                });

                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Données de revenus mensuels (12 derniers mois)
        /// </summary>
        private async Task<object> GetMonthlyRevenueData()
        {
            var last12Months = Enumerable.Range(0, 12)
                .Select(i => DateTime.Now.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var data = await Task.WhenAll(last12Months.Select(async month =>
            {
                var revenue = await _context.InvoiceHistories
                    .Where(i => i.CreatedAt.Year == month.Year && i.CreatedAt.Month == month.Month && i.Status == "Processed")
                    .SumAsync(i => i.TotalAmount);

                return new
                {
                    month = month.ToString("MMM yyyy"),
                    revenue = Math.Round(revenue, 2),
                    monthNum = month.Month,
                    yearNum = month.Year
                };
            }));

            return data;
        }

        /// <summary>
        /// Revenu par statut de facture
        /// </summary>
        private async Task<object> GetRevenueByStatus()
        {
            var statuses = new[] { "Processed", "Draft", "Error" };
            var data = await Task.WhenAll(statuses.Select(async status =>
            {
                var revenue = await _context.InvoiceHistories
                    .Where(i => i.Status == status)
                    .SumAsync(i => i.TotalAmount);

                var count = await _context.InvoiceHistories
                    .Where(i => i.Status == status)
                    .CountAsync();

                return new
                {
                    status = status,
                    revenue = Math.Round(revenue, 2),
                    count = count,
                    percentage = count > 0 ? Math.Round(((decimal)count / await _context.InvoiceHistories.CountAsync()) * 100, 1) : 0
                };
            }));

            return data;
        }

        /// <summary>
        /// Top 10 clients par revenu
        /// </summary>
        private async Task<object> GetTopCustomersByRevenue()
        {
            var topCustomers = await _context.InvoiceHistories
                .AsNoTracking()
                .GroupBy(i => new { i.CustomerName, i.Email })
                .Select(g => new
                {
                    customer = g.Key.CustomerName,
                    email = g.Key.Email,
                    revenue = g.Sum(i => i.TotalAmount),
                    invoiceCount = g.Count()
                })
                .OrderByDescending(x => x.revenue)
                .Take(10)
                .ToListAsync();

            return topCustomers;
        }

        /// <summary>
        /// Nombre de factures par mois
        /// </summary>
        private async Task<object> GetInvoicesByMonth()
        {
            var last12Months = Enumerable.Range(0, 12)
                .Select(i => DateTime.Now.AddMonths(-i))
                .OrderBy(d => d)
                .ToList();

            var data = await Task.WhenAll(last12Months.Select(async month =>
            {
                var count = await _context.InvoiceHistories
                    .Where(i => i.CreatedAt.Year == month.Year && i.CreatedAt.Month == month.Month)
                    .CountAsync();

                return new
                {
                    month = month.ToString("MMM"),
                    count = count
                };
            }));

            return data;
        }

        /// <summary>
        /// Valeur moyenne des factures
        /// </summary>
        private async Task<object> GetAverageInvoiceValue()
        {
            var total = await _context.InvoiceHistories
                .SumAsync(i => i.TotalAmount);

            var count = await _context.InvoiceHistories.CountAsync();

            return new
            {
                average = count > 0 ? Math.Round(total / count, 2) : 0,
                total = Math.Round(total, 2),
                count = count
            };
        }

        /// <summary>
        /// Nombre total de transactions
        /// </summary>
        private async Task<object> GetTotalTransactions()
        {
            var total = await _context.InvoiceHistories.CountAsync();
            var processed = await _context.InvoiceHistories.Where(i => i.Status == "Processed").CountAsync();
            var draft = await _context.InvoiceHistories.Where(i => i.Status == "Draft").CountAsync();
            var error = await _context.InvoiceHistories.Where(i => i.Status == "Error").CountAsync();

            return new
            {
                total = total,
                processed = processed,
                draft = draft,
                error = error,
                successRate = total > 0 ? Math.Round(((decimal)processed / total) * 100, 1) : 0
            };
        }

        /// <summary>
        /// Métriques de conversion et performance
        /// </summary>
        private async Task<object> GetConversionMetrics()
        {
            var last30Days = DateTime.Now.AddDays(-30);
            var today = DateTime.Now;

            var thisMonthCount = await _context.InvoiceHistories
                .Where(i => i.CreatedAt >= last30Days && i.CreatedAt <= today)
                .CountAsync();

            var thisMonthRevenue = await _context.InvoiceHistories
                .Where(i => i.CreatedAt >= last30Days && i.CreatedAt <= today && i.Status == "Processed")
                .SumAsync(i => i.TotalAmount);

            var lastMonthCount = await _context.InvoiceHistories
                .Where(i => i.CreatedAt >= last30Days.AddMonths(-1) && i.CreatedAt < last30Days)
                .CountAsync();

            var growthRate = lastMonthCount > 0 
                ? Math.Round(((decimal)(thisMonthCount - lastMonthCount) / lastMonthCount) * 100, 1) 
                : 0;

            return new
            {
                thisMonthCount = thisMonthCount,
                thisMonthRevenue = Math.Round(thisMonthRevenue, 2),
                lastMonthCount = lastMonthCount,
                growthRate = growthRate,
                avgInvoiceThisMonth = thisMonthCount > 0 ? Math.Round(thisMonthRevenue / thisMonthCount, 2) : 0
            };
        }

        /// <summary>
        /// Récupère toutes les données analytiques
        /// </summary>
        private async Task<dynamic> GetAnalyticsData()
        {
            return new
            {
                monthlyRevenue = await GetMonthlyRevenueData(),
                revenueByStatus = await GetRevenueByStatus(),
                topCustomers = await GetTopCustomersByRevenue(),
                invoicesByMonth = await GetInvoicesByMonth(),
                averageInvoiceValue = await GetAverageInvoiceValue(),
                totalTransactions = await GetTotalTransactions(),
                conversionMetrics = await GetConversionMetrics()
            };
        }
    }
}
