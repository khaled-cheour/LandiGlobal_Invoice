using ClosedXML.Excel;
using LandiGlobalTemplate.Data;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace LandiGlobalTemplate.Services
{
    /// <summary>
    /// Service d'export des données vers Excel et CSV
    /// </summary>
    public class ExportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ExportService> _logger;

        public ExportService(ApplicationDbContext context, ILogger<ExportService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Exporte les factures en Excel
        /// </summary>
        public async Task<byte[]> ExportInvoicesToExcelAsync(List<InvoiceHistory>? invoices = null, string fileName = "Factures")
        {
            try
            {
                invoices ??= await _context.InvoiceHistories.AsNoTracking().ToListAsync();

                using (var workbook = new XLWorkbook())
                {
                    // Feuille 1: Factures
                    var wsInvoices = workbook.Worksheets.Add("Factures");
                    ExportInvoicesSheet(wsInvoices, invoices);

                    // Feuille 2: Produits
                    var wsProducts = workbook.Worksheets.Add("Produits");
                    await ExportProductsSheetAsync(wsProducts, invoices);

                    // Feuille 3: Résumé
                    var wsSummary = workbook.Worksheets.Add("Résumé");
                    ExportSummarySheet(wsSummary, invoices);

                    // Sauvegarder en mémoire
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return stream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export Excel");
                throw;
            }
        }

        /// <summary>
        /// Exporte les factures en CSV
        /// </summary>
        public async Task<byte[]> ExportInvoicesAsCSVAsync(List<InvoiceHistory>? invoices = null)
        {
            try
            {
                invoices ??= await _context.InvoiceHistories.AsNoTracking().ToListAsync();

                var csv = new StringBuilder();

                // En-têtes
                csv.AppendLine("ID,N° Facture,Commande,Date Facture,Client,Email,Téléphone,Montant Total,Devise,Statut," +
                    "Date Création,Validée,Nb Produits,Adresse Facturation,Adresse Livraison,Devise,Incoterms,Terme Paiement");

                // Données
                foreach (var invoice in invoices)
                {
                    var escapedBilling = EscapeCSV(invoice.BillingAddress);
                    var escapedShipping = EscapeCSV(invoice.ShipToAddress);
                    var escapedCustomer = EscapeCSV(invoice.CustomerName);

                    csv.AppendLine($"{invoice.Id},{invoice.InvoiceNumber},{invoice.PurchaseOrderNumber}," +
                        $"{invoice.InvoiceDate:yyyy-MM-dd},{escapedCustomer},{invoice.Email},{invoice.Phone}," +
                        $"{invoice.TotalAmount},{invoice.Currency},{invoice.Status}," +
                        $"{invoice.CreatedAt:yyyy-MM-dd HH:mm:ss},{(invoice.IsValidated ? "Oui" : "Non")},{invoice.ProductCount}," +
                        $"{escapedBilling},{escapedShipping},{invoice.Currency},{invoice.Incoterms},{invoice.PaymentTerm}");
                }

                return Encoding.UTF8.GetBytes(csv.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export CSV");
                throw;
            }
        }

        /// <summary>
        /// Exporte les produits en CSV
        /// </summary>
        public async Task<byte[]> ExportProductsAsCSVAsync()
        {
            try
            {
                var invoices = await _context.InvoiceHistories
                    .Include(x => x.Products)
                    .AsNoTracking()
                    .ToListAsync();

                var csv = new StringBuilder();

                // En-têtes
                csv.AppendLine("N° Facture,N° Produit,Famille,Description,Quantité,Prix Unitaire,Total," +
                    "Code HS,Mémoire,G4,GMS");

                // Données
                foreach (var invoice in invoices)
                {
                    foreach (var product in invoice.Products ?? new List<InvoiceHistoryProduct>())
                    {
                        var escapedDesc = EscapeCSV(product.ProductDescription);
                        var escapedFamily = EscapeCSV(product.FamilyName);

                        csv.AppendLine($"{invoice.InvoiceNumber},{product.ProductNumber},{escapedFamily}," +
                            $"{escapedDesc},{product.Quantity},{product.UnitPrice:F2},{product.TotalAmount:F2}," +
                            $"{product.HSCode},{product.MemoryPlan},{product.G4},{product.GMS}");
                    }
                }

                return Encoding.UTF8.GetBytes(csv.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export des produits CSV");
                throw;
            }
        }

        /// <summary>
        /// Crée une feuille Excel avec les factures
        /// </summary>
        private void ExportInvoicesSheet(IXLWorksheet ws, List<InvoiceHistory> invoices)
        {
            // En-têtes
            var headers = new[] { "ID", "N° Facture", "Client", "Email", "Montant", "Devise", "Statut", 
                "Validée", "Nb Produits", "Date Création", "Terme Paiement" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Données
            int row = 2;
            foreach (var invoice in invoices)
            {
                ws.Cell(row, 1).Value = invoice.Id;
                ws.Cell(row, 2).Value = invoice.InvoiceNumber;
                ws.Cell(row, 3).Value = invoice.CustomerName;
                ws.Cell(row, 4).Value = invoice.Email;
                ws.Cell(row, 5).Value = invoice.TotalAmount;
                ws.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00\"$\"";
                ws.Cell(row, 6).Value = invoice.Currency;
                ws.Cell(row, 7).Value = invoice.Status;
                ws.Cell(row, 8).Value = invoice.IsValidated ? "Oui" : "Non";
                ws.Cell(row, 9).Value = invoice.ProductCount;
                ws.Cell(row, 10).Value = invoice.CreatedAt;
                ws.Cell(row, 10).Style.DateFormat.Format = "yyyy-mm-dd hh:mm:ss";
                ws.Cell(row, 11).Value = invoice.PaymentTerm;

                row++;
            }

            // Ajuster les largeurs
            ws.Columns().AdjustToContents();
        }

        /// <summary>
        /// Crée une feuille Excel avec les produits
        /// </summary>
        private async Task ExportProductsSheetAsync(IXLWorksheet ws, List<InvoiceHistory> invoices)
        {
            var invoicesWithProducts = invoices.Where(x => x.Products?.Count > 0).ToList();

            // En-têtes
            var headers = new[] { "Facture", "N° Produit", "Famille", "Description", "Quantité", 
                "Prix Unitaire", "Total", "Code HS", "Origine" };
            
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGreen;
            }

            // Données
            int row = 2;
            foreach (var invoice in invoicesWithProducts)
            {
                foreach (var product in invoice.Products ?? new List<InvoiceHistoryProduct>())
                {
                    ws.Cell(row, 1).Value = invoice.InvoiceNumber;
                    ws.Cell(row, 2).Value = product.ProductNumber;
                    ws.Cell(row, 3).Value = product.FamilyName;
                    ws.Cell(row, 4).Value = product.ProductDescription;
                    ws.Cell(row, 5).Value = product.Quantity;
                    ws.Cell(row, 6).Value = product.UnitPrice;
                    ws.Cell(row, 6).Style.NumberFormat.Format = "#,##0.00\"$\"";
                    ws.Cell(row, 7).Value = product.TotalAmount;
                    ws.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00\"$\"";
                    ws.Cell(row, 8).Value = product.HSCode;
                    ws.Cell(row, 9).Value = "";

                    row++;
                }
            }

            ws.Columns().AdjustToContents();
        }

        /// <summary>
        /// Crée une feuille Excel avec un résumé
        /// </summary>
        private void ExportSummarySheet(IXLWorksheet ws, List<InvoiceHistory> invoices)
        {
            ws.Cell(1, 1).Value = "RÉSUMÉ DES FACTURES";
            ws.Cell(1, 1).Style.Font.Bold = true;
            ws.Cell(1, 1).Style.Font.FontSize = 14;

            int row = 3;

            // Statistiques générales
            ws.Cell(row, 1).Value = "Total Factures:";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = invoices.Count;
            row += 2;

            ws.Cell(row, 1).Value = "Revenu Total:";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = invoices.Sum(x => x.TotalAmount);
            ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00\"$\"";
            row += 2;

            ws.Cell(row, 1).Value = "Montant Moyen:";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = invoices.Average(x => x.TotalAmount);
            ws.Cell(row, 2).Style.NumberFormat.Format = "#,##0.00\"$\"";
            row += 2;

            // Par statut
            ws.Cell(row, 1).Value = "Factures Traitées:";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = invoices.Count(x => x.Status == "Processed");
            row += 1;

            ws.Cell(row, 1).Value = "Factures en Brouillon:";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = invoices.Count(x => x.Status == "Draft");
            row += 1;

            ws.Cell(row, 1).Value = "Factures en Erreur:";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = invoices.Count(x => x.Status == "Error");

            ws.Columns().AdjustToContents();
        }

        /// <summary>
        /// Échappe les caractères spéciaux pour CSV
        /// </summary>
        private string EscapeCSV(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }
    }
}
