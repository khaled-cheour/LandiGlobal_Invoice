using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using LandiGlobalTemplate.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LandiGlobalTemplate.Services
{
    public class PDFExtractionService
    {
        public CommercialInvoice ExtractFromPDF(byte[] pdfData)
        {
            var invoice = new CommercialInvoice();
            var text = ExtractTextFromPDF(pdfData);
            
            // Parse invoice information
            invoice.InvoiceInfo = ParseInvoiceInformation(text);
            
            // Parse customer information
            invoice.CustomerInfo = ParseCustomerInformation(text);
            
            // Parse products
            invoice.Products = ParseProducts(text);
            
            // Calculate total
            invoice.TotalAmount = invoice.Products.Sum(p => p.TotalAmount);
            ValidateExtraction(text, invoice);
            
            return invoice;
        }

        private string ExtractTextFromPDF(byte[] pdfData)
        {
            using (var memoryStream = new MemoryStream(pdfData))
            {
                using (var reader = new PdfReader(memoryStream))
                {
                    using (var document = new PdfDocument(reader))
                    {
                        var text = new System.Text.StringBuilder();
                        var strategy = new LocationTextExtractionStrategy();

                        for (int i = 1; i <= document.GetNumberOfPages(); i++)
                        {
                            var page = document.GetPage(i);
                            var currentText = PdfTextExtractor.GetTextFromPage(page, strategy);
                            text.Append(currentText);
                            text.Append("\n");
                        }

                        return text.ToString();
                    }
                }
            }
        }

        private InvoiceInformation ParseInvoiceInformation(string text)
        {
            var info = new InvoiceInformation();

            // Extract Invoice Number
            var invoiceMatch = Regex.Match(text, @"Invoice Number\s*[:=]?\s*([S0-9\-]+)", RegexOptions.IgnoreCase);
            if (invoiceMatch.Success)
                info.InvoiceNumber = invoiceMatch.Groups[1].Value.Trim();

            // Extract Invoice Date
            var dateMatch = Regex.Match(text, @"Invoice Date\s*[:=]?\s*(\d{1,2}/\d{1,2}/\d{4})", RegexOptions.IgnoreCase);
            if (dateMatch.Success)
                info.InvoiceDate = dateMatch.Groups[1].Value.Trim();

            // Extract Purchase Order Number
            var poMatch = Regex.Match(text, @"Purchase Order Number\s*[:=]?\s*([A-Z0-9\-]+)", RegexOptions.IgnoreCase);
            if (poMatch.Success)
                info.PurchaseOrderNumber = poMatch.Groups[1].Value.Trim();

            // Extract Payment Term
            var paymentMatch = Regex.Match(text, @"Payment Term\s*[:=]?\s*(.+?)(?=\n|Inco)", RegexOptions.IgnoreCase);
            if (paymentMatch.Success)
                info.PaymentTerm = paymentMatch.Groups[1].Value.Trim();

            // Extract Incoterms
            var incotermsMatch = Regex.Match(text, @"Incoterms\s*[:=]?\s*([A-Z]+)", RegexOptions.IgnoreCase);
            if (incotermsMatch.Success)
                info.Incoterms = incotermsMatch.Groups[1].Value.Trim();

            // Extract Country of Origin
            var countryMatch = Regex.Match(text, @"COUNTRY OF ORIGIN\s*[:=]?\s*([A-Z]+)", RegexOptions.IgnoreCase);
            if (countryMatch.Success)
                info.CountryOfOrigin = countryMatch.Groups[1].Value.Trim();
            else
                info.CountryOfOrigin = "CHINA";

            return info;
        }

        private CustomerInformation ParseCustomerInformation(string text)
        {
            var info = new CustomerInformation();

            // Default values - Company Info
            const string DEFAULT_SHIP_FROM = "Wanlida Industrial Zone, Jingcheng town, Nanjing, Zhangzhou, Fujian, 363600, China";
            const string DEFAULT_SHIP_TO = "Weijerbeemd 12, 5651GN EINDHOVEN, The Netherlands";
            const string DEFAULT_EMAIL = "ap-eu@eu.bluestarinc.com";
            const string DEFAULT_VAT = "NL852024551B01";
            const string DEFAULT_EORI = "NL852024551";

            // Extract Customer Name
            var customerMatch = Regex.Match(text, @"Customer Name\s*[:=]?\s*(.+?)(?=\n|Billing)", RegexOptions.IgnoreCase);
            if (customerMatch.Success)
                info.CustomerName = customerMatch.Groups[1].Value.Trim();

            // Extract Billing Address
            var billingMatch = Regex.Match(text, @"Billing Address\s*[:=]?\s*(.+?)(?=\n|Ship to|Email|VAT)", RegexOptions.IgnoreCase);
            if (billingMatch.Success)
                info.BillingAddress = billingMatch.Groups[1].Value.Trim();

            // Extract Ship to address
            var shipToMatch = Regex.Match(text, @"Ship to address\s*[:=]?\s*(.+?)(?=\n|Ship From|Email)", RegexOptions.IgnoreCase);
            if (shipToMatch.Success)
                info.ShipToAddress = shipToMatch.Groups[1].Value.Trim();
            else
                info.ShipToAddress = DEFAULT_SHIP_TO; // Use default if not found

            // Extract Ship From Address (with default)
            var shipFromMatch = Regex.Match(text, @"Ship From Address\s*[:=]?\s*(.+?)(?=\n|Email)", RegexOptions.IgnoreCase);
            if (shipFromMatch.Success)
                info.ShipFromAddress = shipFromMatch.Groups[1].Value.Trim();
            else
                info.ShipFromAddress = DEFAULT_SHIP_FROM; // Use default if not found

            // Extract Email (with default)
            var emailMatch = Regex.Match(text, @"Email\s*[:=]?\s*([^\s]+@[^\s]+)", RegexOptions.IgnoreCase);
            if (emailMatch.Success)
                info.Email = emailMatch.Groups[1].Value.Trim();
            else
                info.Email = DEFAULT_EMAIL; // Use default if not found

            // Extract Phone
            var phoneMatch = Regex.Match(text, @"Phone\s*[:=]?\s*(\+?\d[\d\s\-\.]+)", RegexOptions.IgnoreCase);
            if (phoneMatch.Success)
                info.Phone = phoneMatch.Groups[1].Value.Trim();

            // Extract VAT (with default)
            var vatMatch = Regex.Match(text, @"VAT\s*[:=]?\s*([A-Z0-9]+)", RegexOptions.IgnoreCase);
            if (vatMatch.Success)
                info.VAT = vatMatch.Groups[1].Value.Trim();
            else
                info.VAT = DEFAULT_VAT; // Use default if not found

            // Extract EORI (with default)
            var eoriMatch = Regex.Match(text, @"EORI\s*[:=]?\s*([A-Z0-9]+)", RegexOptions.IgnoreCase);
            if (eoriMatch.Success)
                info.EORI = eoriMatch.Groups[1].Value.Trim();
            else
                info.EORI = DEFAULT_EORI; // Use default if not found

            return info;
        }

        private List<ProductInformation> ParseProducts(string text)
        {
            var products = new List<ProductInformation>();
            var productLines = ExtractProductLines(text);

            // Parse each product line
            foreach (var line in productLines)
            {
                var product = ParseProductLine(line);
                if (IsProductLineCandidate(product))
                {
                    if (ProductCatalog.IsValidProductNumber(product.ProductNumber))
                    {
                        ProductCatalog.EnrichProductInfo(product);
                    }

                    products.Add(product);
                }
            }

            return products;
        }

        private List<string> ExtractProductLines(string text)
        {
            var lines = text.Split('\n');
            var inProductSection = false;
            var productLines = new List<string>();

            foreach (var line in lines)
            {
                if (line.Contains("Product Number", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("Product description", StringComparison.OrdinalIgnoreCase))
                {
                    inProductSection = true;
                    continue;
                }

                if (inProductSection && (line.Contains("Total", StringComparison.OrdinalIgnoreCase) ||
                    line.Contains("Payment Term", StringComparison.OrdinalIgnoreCase)))
                {
                    break;
                }

                if (inProductSection && !string.IsNullOrWhiteSpace(line))
                {
                    productLines.Add(line.Trim());
                }
            }

            return productLines;
        }

        private void ValidateExtraction(string text, CommercialInvoice invoice)
        {
            invoice.PdfProductCount = CountProductRowsInPdf(text);
            invoice.PdfTotalAmount = ExtractPdfTotalAmount(text);

            if (invoice.PdfProductCount > 0 && invoice.Products.Count < invoice.PdfProductCount)
            {
                invoice.ExtractionErrors.Add(
                    $"Il y a un problème sur votre facture : le PDF contient {invoice.PdfProductCount} produit(s), mais seulement {invoice.Products.Count} produit(s) ont été extraits.");
            }

            if (invoice.PdfTotalAmount.HasValue && Math.Abs(invoice.PdfTotalAmount.Value - invoice.TotalAmount) > 0.01m)
            {
                invoice.ExtractionErrors.Add(
                    $"Il y a un problème sur votre facture : le total du PDF ({invoice.PdfTotalAmount.Value:F2}) est différent du total extrait ({invoice.TotalAmount:F2}).");
            }
        }

        private int CountProductRowsInPdf(string text)
        {
            return ExtractProductLines(text)
                .Select(ParseProductLine)
                .Count(IsProductRowInPdf);
        }

        private bool IsProductRowInPdf(ProductInformation product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductNumber))
                return false;

            if (product.Quantity <= 0 || product.UnitPrice <= 0)
                return false;

            return Regex.IsMatch(product.ProductNumber.Trim(), @"^(?=.*\d)[A-Z0-9][A-Z0-9\-]{5,}$", RegexOptions.IgnoreCase);
        }

        private decimal? ExtractPdfTotalAmount(string text)
        {
            var lines = text.Split('\n');

            foreach (var line in lines)
            {
                var match = Regex.Match(
                    line.Trim(),
                    @"^Total\s+(?:[A-Z]{3}\s+)?(?<amount>[\d,]+(?:\.\d{1,2})?)$",
                    RegexOptions.IgnoreCase);

                if (match.Success && TryParseDecimal(match.Groups["amount"].Value, out var total))
                    return total;
            }

            return null;
        }

        private bool IsProductLineCandidate(ProductInformation product)
        {
            if (string.IsNullOrWhiteSpace(product.ProductNumber))
                return false;

            var productNumber = product.ProductNumber.Trim();

            if (ProductCatalog.IsValidProductNumber(productNumber))
                return true;

            if (product.Quantity <= 0 && product.UnitPrice <= 0 && product.TotalAmount <= 0)
                return false;

            // Accept product codes that are not yet in the static catalog, e.g. WX01000012.
            return Regex.IsMatch(productNumber, @"^(?=.*\d)[A-Z0-9][A-Z0-9\-]{5,}$", RegexOptions.IgnoreCase);
        }

        private ProductInformation ParseProductLine(string line)
        {
            var product = new ProductInformation();
            
            // Clean up the line
            var cleanLine = line.Replace("\t", " ").Trim();
            var parts = Regex.Split(cleanLine, @"\s+").Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
            
            if (parts.Length == 0)
                return product;

            // First part is usually product number
            product.ProductNumber = parts[0];

            // Extract numeric values (quantity, unit price, total amount)
            var numericValues = new List<decimal>();
            var numericIndices = new List<int>();
            
            for (int i = 0; i < parts.Length; i++)
            {
                if (TryParseDecimal(parts[i], out var value))
                {
                    numericValues.Add(value);
                    numericIndices.Add(i);
                }
            }

            // Try to extract quantity, unit price, and total amount
            // Usually pattern: ProductNumber ... Quantity UnitPrice TotalAmount HSCode
            if (numericIndices.Count >= 2)
            {
                // Assume last numeric value before HSCode is TotalAmount
                // Second to last is UnitPrice
                // Third to last is Quantity
                
                if (numericValues.Count >= 3)
                {
                    product.Quantity = (int)numericValues[numericValues.Count - 3];
                    product.UnitPrice = numericValues[numericValues.Count - 2];
                    product.TotalAmount = numericValues[numericValues.Count - 1];
                }
                else if (numericValues.Count == 2)
                {
                    // If only 2 numbers, assume they are quantity and unit price
                    product.Quantity = (int)numericValues[0];
                    product.UnitPrice = numericValues[1];
                    product.TotalAmount = product.Quantity * product.UnitPrice;
                }
                else if (numericValues.Count == 1)
                {
                    product.UnitPrice = numericValues[0];
                    product.Quantity = 1;
                    product.TotalAmount = product.UnitPrice;
                }
            }

            // Extract HS Code if present (8-10 digits usually at the end)
            var hsMatch = Regex.Match(line, @"\b(\d{8,10})\b");
            if (hsMatch.Success)
                product.HSCode = hsMatch.Groups[1].Value;

            // Extract description from parts between product number and first numeric value
            if (numericIndices.Count > 0)
            {
                var descriptionEndIndex = numericIndices[0];
                if (descriptionEndIndex > 1)
                {
                    var descriptionParts = parts.Skip(1).Take(descriptionEndIndex - 1).ToArray();
                    product.ProductDescription = string.Join(" ", descriptionParts);
                }
            }

            return product;
        }

        private bool TryParseDecimal(string value, out decimal result)
        {
            return decimal.TryParse(
                value,
                NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out result);
        }
    }
}
