using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LandiGlobalTemplate.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LandiGlobalTemplate.Controllers
{
    [Authorize]
    [Route("[controller]")]
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Page de recherche avancée
        /// </summary>
        public async Task<IActionResult> Advanced(string invoiceNumber, string customerName, string customerEmail, 
            string status, decimal? minAmount, decimal? maxAmount, DateTime? startDate, DateTime? endDate, 
            string currency, string paymentTerms, int page = 1, int pageSize = 20)
        {
            var query = _context.InvoiceHistories.AsNoTracking().AsQueryable();

            // Filtres
            if (!string.IsNullOrWhiteSpace(invoiceNumber))
                query = query.Where(i => i.InvoiceNumber.Contains(invoiceNumber));

            if (!string.IsNullOrWhiteSpace(customerName))
                query = query.Where(i => i.CustomerName.Contains(customerName));

            if (!string.IsNullOrWhiteSpace(customerEmail))
                query = query.Where(i => i.Email.Contains(customerEmail));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(i => i.Status == status);

            if (minAmount.HasValue)
                query = query.Where(i => i.TotalAmount >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(i => i.TotalAmount <= maxAmount.Value);

            if (startDate.HasValue)
                query = query.Where(i => i.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(i => i.CreatedAt <= endDate.Value.AddDays(1));

            if (!string.IsNullOrWhiteSpace(currency))
                query = query.Where(i => i.Currency == currency);

            if (!string.IsNullOrWhiteSpace(paymentTerms))
                query = query.Where(i => i.PaymentTerm == paymentTerms);

            var totalCount = await query.CountAsync();
            var skip = (page - 1) * pageSize;

            var results = await query
                .OrderByDescending(i => i.CreatedAt)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalCount = totalCount;
            ViewBag.PageSize = pageSize;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.InvoiceNumber = invoiceNumber;
            ViewBag.CustomerName = customerName;
            ViewBag.CustomerEmail = customerEmail;
            ViewBag.Status = status;
            ViewBag.MinAmount = minAmount;
            ViewBag.MaxAmount = maxAmount;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Currency = currency;
            ViewBag.PaymentTerms = paymentTerms;

            return View(results);
        }

        /// <summary>
        /// API endpoint pour recherche avancée avec filtres JSON
        /// </summary>
        [HttpPost("api/search")]
        [Produces("application/json")]
        public async Task<IActionResult> ApiSearch([FromBody] SearchFilterRequest filter)
        {
            try
            {
                var query = _context.InvoiceHistories.AsNoTracking().AsQueryable();

                // Appliquer les filtres
                if (!string.IsNullOrWhiteSpace(filter.InvoiceNumber))
                    query = query.Where(i => i.InvoiceNumber.Contains(filter.InvoiceNumber));

                if (!string.IsNullOrWhiteSpace(filter.CustomerName))
                    query = query.Where(i => i.CustomerName.Contains(filter.CustomerName));

                if (!string.IsNullOrWhiteSpace(filter.CustomerEmail))
                    query = query.Where(i => i.Email.Contains(filter.CustomerEmail));

                if (!string.IsNullOrWhiteSpace(filter.Status))
                    query = query.Where(i => i.Status == filter.Status);

                if (filter.MinAmount.HasValue)
                    query = query.Where(i => i.TotalAmount >= filter.MinAmount.Value);

                if (filter.MaxAmount.HasValue)
                    query = query.Where(i => i.TotalAmount <= filter.MaxAmount.Value);

                if (filter.StartDate.HasValue)
                    query = query.Where(i => i.CreatedAt >= filter.StartDate.Value);

                if (filter.EndDate.HasValue)
                    query = query.Where(i => i.CreatedAt <= filter.EndDate.Value.AddDays(1));

                if (!string.IsNullOrWhiteSpace(filter.Currency))
                    query = query.Where(i => i.Currency == filter.Currency);

                if (!string.IsNullOrWhiteSpace(filter.PaymentTerms))
                    query = query.Where(i => i.PaymentTerm == filter.PaymentTerms);

                var totalCount = await query.CountAsync();
                var page = filter.Page ?? 1;
                var pageSize = filter.PageSize ?? 20;
                var skip = (page - 1) * pageSize;

                var results = await query
                    .OrderByDescending(i => i.CreatedAt)
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(i => new
                    {
                        i.Id,
                        i.InvoiceNumber,
                        i.CustomerName,
                        i.Email,
                        i.TotalAmount,
                        i.Status,
                        i.CreatedAt,
                        i.Currency,
                        i.PaymentTerm
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = results,
                    pagination = new
                    {
                        totalCount = totalCount,
                        currentPage = page,
                        pageSize = pageSize,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Retour des valeurs disponibles pour les filtres
        /// </summary>
        [HttpGet("api/filter-options")]
        [Produces("application/json")]
        public async Task<IActionResult> GetFilterOptions()
        {
            try
            {
                var statuses = await _context.InvoiceHistories
                    .Select(i => i.Status)
                    .Distinct()
                    .OrderBy(s => s)
                    .ToListAsync();

                var currencies = await _context.InvoiceHistories
                    .Select(i => i.Currency)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                var paymentTerms = await _context.InvoiceHistories
                    .Select(i => i.PaymentTerm)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToListAsync();

                var minAmount = await _context.InvoiceHistories
                    .MinAsync(i => (decimal?)i.TotalAmount) ?? 0;

                var maxAmount = await _context.InvoiceHistories
                    .MaxAsync(i => (decimal?)i.TotalAmount) ?? 0;

                return Json(new
                {
                    statuses = statuses,
                    currencies = currencies,
                    paymentTerms = paymentTerms,
                    amountRange = new { min = Math.Round(minAmount, 2), max = Math.Round(maxAmount, 2) }
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Filtre de recherche avancée
    /// </summary>
    public class SearchFilterRequest
    {
        public string? InvoiceNumber { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? Status { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Currency { get; set; }
        public string? PaymentTerms { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
    }
}
