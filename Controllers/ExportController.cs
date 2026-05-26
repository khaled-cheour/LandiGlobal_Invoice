using LandiGlobalTemplate.Data;
using LandiGlobalTemplate.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LandiGlobalTemplate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ExportController : ControllerBase
    {
        private readonly ExportService _exportService;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExportController> _logger;

        public ExportController(ExportService exportService, ApplicationDbContext context, ILogger<ExportController> logger)
        {
            _exportService = exportService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Exporte toutes les factures en Excel
        /// </summary>
        [HttpGet("invoices/excel")]
        public async Task<IActionResult> ExportInvoicesExcel()
        {
            try
            {
                _logger.LogInformation($"Export Excel des factures demandé par {User?.Identity?.Name}");
                
                var excelFile = await _exportService.ExportInvoicesToExcelAsync();
                var fileName = $"Factures_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

                return File(excelFile, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export Excel");
                return BadRequest(new { message = "Erreur lors de l'export Excel", error = ex.Message });
            }
        }

        /// <summary>
        /// Exporte toutes les factures en CSV
        /// </summary>
        [HttpGet("invoices/csv")]
        public async Task<IActionResult> ExportInvoicesCSV()
        {
            try
            {
                _logger.LogInformation($"Export CSV des factures demandé par {User?.Identity?.Name}");
                
                var csvFile = await _exportService.ExportInvoicesAsCSVAsync();
                var fileName = $"Factures_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

                return File(csvFile, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export CSV");
                return BadRequest(new { message = "Erreur lors de l'export CSV", error = ex.Message });
            }
        }

        /// <summary>
        /// Exporte les produits en CSV
        /// </summary>
        [HttpGet("products/csv")]
        public async Task<IActionResult> ExportProductsCSV()
        {
            try
            {
                _logger.LogInformation($"Export CSV des produits demandé par {User?.Identity?.Name}");
                
                var csvFile = await _exportService.ExportProductsAsCSVAsync();
                var fileName = $"Produits_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

                return File(csvFile, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export CSV des produits");
                return BadRequest(new { message = "Erreur lors de l'export CSV des produits", error = ex.Message });
            }
        }

        /// <summary>
        /// Exporte les factures filtrées en Excel
        /// </summary>
        [HttpPost("invoices/excel/filtered")]
        public async Task<IActionResult> ExportFilteredInvoicesExcel([FromBody] ExportFilterRequest request)
        {
            try
            {
                var query = _context.InvoiceHistories.AsNoTracking();

                // Appliquer les filtres
                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    query = query.Where(x => x.Status == request.Status);
                }

                if (!string.IsNullOrWhiteSpace(request.CustomerName))
                {
                    query = query.Where(x => x.CustomerName.Contains(request.CustomerName));
                }

                if (request.StartDate.HasValue)
                {
                    query = query.Where(x => x.CreatedAt >= request.StartDate.Value);
                }

                if (request.EndDate.HasValue)
                {
                    query = query.Where(x => x.CreatedAt <= request.EndDate.Value);
                }

                var invoices = await query.ToListAsync();
                var excelFile = await _exportService.ExportInvoicesToExcelAsync(invoices);
                var fileName = $"Factures_Filtrees_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";

                return File(excelFile, 
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                    fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export Excel filtré");
                return BadRequest(new { message = "Erreur lors de l'export Excel", error = ex.Message });
            }
        }
    }

    /// <summary>
    /// Requête de filtre pour l'export
    /// </summary>
    public class ExportFilterRequest
    {
        public string? Status { get; set; }
        public string? CustomerName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
