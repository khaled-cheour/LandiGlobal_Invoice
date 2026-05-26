using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Colors;
using iText.Layout;
using iText.Layout.Borders;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using LandiGlobalTemplate.Models;

namespace LandiGlobalTemplate.Services
{
    public class PDFGenerationService
    {
        private const string HELVETICA = "Helvetica";
        private const string HELVETICA_BOLD = "Helvetica-Bold";

        // Colors
        private static readonly DeviceRgb HEADER_BG = new DeviceRgb(166, 166, 166);        // #A6A6A6
        private static readonly DeviceRgb LABEL_BG = new DeviceRgb(217, 217, 217);         // #D9D9D9
        private static readonly DeviceRgb TOTAL_BG = new DeviceRgb(242, 242, 242);        // #F2F2F2
        private static readonly DeviceRgb BORDER_COLOR = new DeviceRgb(191, 191, 191);    // #BFBFBF
        private static readonly DeviceRgb TEXT_BLACK = new DeviceRgb(0, 0, 0);            // #000000
        private static readonly DeviceRgb TITLE_BLUE = new DeviceRgb(100, 150, 200);      // Bleu clair
        private static readonly DeviceRgb SEPARATOR_GRAY = new DeviceRgb(180, 180, 180);  // Gris séparation

        private const string DEFAULT_PAYMENT_TERM = "45 days after invoice";
        private const string DEFAULT_INCOTERMS = "CIP";
        private const string DEFAULT_COUNTRY_OF_ORIGIN = "CHINA";
        private const string DEFAULT_SHIP_TO = "Weijerbeemd 12, 5651GN EINDHOVEN, The Netherlands";
        private const string DEFAULT_SHIP_FROM = "Wanlida Industrial Zone, Jingcheng town, Nanjing, Zhangzhou, Fujian, 363600, China";

        // Column widths (in points) - Total 515 pt
        private static readonly float[] COLUMN_WIDTHS = { 90f, 170f, 60f, 75f, 75f, 45f };

        public byte[] GenerateInvoicePDF(CommercialInvoice invoice)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new PdfWriter(memoryStream))
                {
                    using (var document = new PdfDocument(writer))
                    {
                        var pdfDocument = new Document(document);
                        pdfDocument.SetMargins(20, 20, 20, 20);

                        // LIGNE 1: TITRE
                        AddTitle(pdfDocument);

                        // LIGNE 2: ESPACE
                        pdfDocument.Add(new Paragraph("").SetHeight(8));

                        // LIGNES 3-6: EN-TÊTE (Logo + Invoice Number)
                        AddHeader(pdfDocument, invoice);

                        // ESPACE
                        pdfDocument.Add(new Paragraph("").SetHeight(10));

                        // SECTION INFOS FACTURE
                        AddInvoiceInformationSection(pdfDocument, invoice);

                        // ESPACE
                        pdfDocument.Add(new Paragraph("").SetHeight(15));

                        // TABLE PRODUITS
                        AddProductsSection(pdfDocument, invoice);

                        // ESPACE
                        pdfDocument.Add(new Paragraph("").SetHeight(12));

                        // SECTION PAIEMENT
                        AddPaymentSection(pdfDocument, invoice);

                        pdfDocument.Close();
                    }
                }

                return memoryStream.ToArray();
            }
        }

        private void AddTitle(Document document)
        {
            var table = new Table(new float[] { 515f });
            var cell = new Cell()
                .SetHeight(20)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetBorder(Border.NO_BORDER)
                .Add(new Paragraph("Commercial Invoice")
                    .SetFont(PdfFontFactory.CreateFont(HELVETICA_BOLD))
                    .SetFontSize(16)
                    .SetFontColor(TITLE_BLUE)
                    .SetTextAlignment(TextAlignment.CENTER));
            table.AddCell(cell);
            document.Add(table);
        }

        private void AddHeader(Document document, CommercialInvoice invoice)
        {
            // Header section with logo/company info on left and invoice number on right
            var mainTable = new Table(new float[] { 360f, 155f }); // Left column wider for company info
            mainTable.SetBorder(Border.NO_BORDER);

            // Left column: Logo and Company Information
            var leftCell = new Cell()
                .SetVerticalAlignment(VerticalAlignment.TOP)
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0);

            var companyTable = new Table(new float[] { 360f });
            companyTable.SetBorder(Border.NO_BORDER);

            // Logo with image
            var logoCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetHeight(80); // Space for logo

            try
            {
                // Load the logo image from the project root
                // Try multiple path variations to ensure we find the logo
                string[] possiblePaths = {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "landi logo.jpg"),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "landi logo.jpg"),
                    "landi logo.jpg"
                };

                string logoPath = null;
                foreach (var path in possiblePaths)
                {
                    var fullPath = Path.GetFullPath(path);
                    if (File.Exists(fullPath))
                    {
                        logoPath = fullPath;
                        break;
                    }
                }

                if (logoPath != null)
                {
                    var image = new Image(ImageDataFactory.Create(logoPath));
                    image.SetHeight(70);
                    image.ScaleToFit(70, 70);
                    logoCell.Add(image);
                }
                else
                {
                    // Fallback to text if image not found
                    logoCell.Add(new Paragraph("LANDI")
                        .SetFont(PdfFontFactory.CreateFont(HELVETICA_BOLD))
                        .SetFontSize(14)
                        .SetFontColor(TEXT_BLACK));
                }
            }
            catch
            {
                // Fallback to text if image loading fails
                logoCell.Add(new Paragraph("LANDI")
                    .SetFont(PdfFontFactory.CreateFont(HELVETICA_BOLD))
                    .SetFontSize(14)
                    .SetFontColor(TEXT_BLACK));
            }

            // Add spacing after logo (2cm = ~56.7 points)
            logoCell.SetMarginBottom(56);
            companyTable.AddCell(logoCell);

            // Company details
            var companyDetailsCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetMarginTop(3)
                .Add(new Paragraph("LANDI INTERNATIONAL (SINGAPORE) PTE. LTD.")
                    .SetFont(PdfFontFactory.CreateFont(HELVETICA_BOLD))
                    .SetFontSize(8)
                    .SetFontColor(TEXT_BLACK));
            companyTable.AddCell(companyDetailsCell);

            // UEN
            var uenCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetMarginTop(1)
                .Add(new Paragraph("UEN: 202325050C")
                    .SetFont(PdfFontFactory.CreateFont(HELVETICA))
                    .SetFontSize(7)
                    .SetFontColor(TEXT_BLACK));
            companyTable.AddCell(uenCell);

            // Address
            var addressCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetMarginTop(1)
                .Add(new Paragraph("152 Beach Road, #37-08, Gateway East, Singapore 189721")
                    .SetFont(PdfFontFactory.CreateFont(HELVETICA))
                    .SetFontSize(7)
                    .SetFontColor(TEXT_BLACK));
            companyTable.AddCell(addressCell);

            leftCell.Add(companyTable);
            mainTable.AddCell(leftCell);

            // Right column: Invoice Number
            var rightCell = new Cell()
                .SetVerticalAlignment(VerticalAlignment.TOP)
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetTextAlignment(TextAlignment.RIGHT);

            var invoiceTable = new Table(new float[] { 155f });
            invoiceTable.SetBorder(Border.NO_BORDER);

            var invoiceLabelCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .Add(new Paragraph("Invoice Number")
                    .SetFont(PdfFontFactory.CreateFont(HELVETICA))
                    .SetFontSize(7)
                    .SetFontColor(TEXT_BLACK)
                    .SetTextAlignment(TextAlignment.RIGHT));
            invoiceTable.AddCell(invoiceLabelCell);

            var invoiceValueCell = new Cell()
                .SetBorder(Border.NO_BORDER)
                .SetPadding(0)
                .SetMarginTop(1)
                .Add(new Paragraph(invoice.InvoiceInfo?.InvoiceNumber ?? "N/A")
                    .SetFont(PdfFontFactory.CreateFont(HELVETICA_BOLD))
                    .SetFontSize(10)
                    .SetFontColor(TEXT_BLACK)
                    .SetTextAlignment(TextAlignment.RIGHT));
            invoiceTable.AddCell(invoiceValueCell);

            rightCell.Add(invoiceTable);
            mainTable.AddCell(rightCell);

            mainTable.SetWidth(UnitValue.CreatePercentValue(100));
            document.Add(mainTable);

            // Add separator line (grey horizontal line)
            var separator = new Paragraph("")
                .SetHeight(1)
                .SetBorder(new SolidBorder(SEPARATOR_GRAY, 1f))
                .SetMarginTop(8)
                .SetMarginBottom(8);
            document.Add(separator);
        }

        private void AddInvoiceInformationSection(Document document, CommercialInvoice invoice)
        {
            var table = new Table(new float[] { 260f, 255f }); // 2 colonnes (label + value)
            table.SetBorder(new SolidBorder(BORDER_COLOR, 0.5f));

            if (invoice?.InvoiceInfo != null)
            {
                AddInfoRow(table, "Invoice Number", invoice.InvoiceInfo.InvoiceNumber);
                AddInfoRow(table, "Invoice Date", invoice.InvoiceInfo.InvoiceDate);
                AddInfoRow(table, "Purchase Order Number", ValueOrDefault(invoice.InvoiceInfo.PurchaseOrderNumber, "-"));
                AddInfoRow(table, "Billing Address", ValueOrDefault(invoice.CustomerInfo?.BillingAddress, "-"));
                AddInfoRow(table, "Ship to Address", ValueOrDefault(invoice.CustomerInfo?.ShipToAddress, DEFAULT_SHIP_TO));
                AddInfoRow(table, "Ship From Address", ValueOrDefault(invoice.CustomerInfo?.ShipFromAddress, DEFAULT_SHIP_FROM));
                AddInfoRow(table, "Email", ValueOrDefault(invoice.CustomerInfo?.Email, "-"));
                AddInfoRow(table, "VAT", ValueOrDefault(invoice.CustomerInfo?.VAT, "-"));
                AddInfoRow(table, "EORI", ValueOrDefault(invoice.CustomerInfo?.EORI, "-"));
            }

            table.SetWidth(UnitValue.CreatePercentValue(100));
            document.Add(table);
        }

        private void AddInfoRow(Table table, string label, string value)
        {
            // Label cell
            var labelCell = new Cell()
                .SetHeight(12)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetBackgroundColor(LABEL_BG)
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetPadding(3)
                .Add(new Paragraph(label)
                    .SetFont(PdfFontFactory.CreateFont(HELVETICA_BOLD))
                    .SetFontSize(7)
                    .SetFontColor(TEXT_BLACK));
            table.AddCell(labelCell);

            // Value cell
            var valueCell = new Cell()
                .SetHeight(12)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetBackgroundColor(ColorConstants.WHITE)
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetPadding(3)
                .Add(new Paragraph(value)
                    .SetFont(PdfFontFactory.CreateFont(HELVETICA))
                    .SetFontSize(7)
                    .SetFontColor(TEXT_BLACK));
            table.AddCell(valueCell);
        }

        private void AddProductsSection(Document document, CommercialInvoice invoice)
        {
            var table = new Table(COLUMN_WIDTHS);
            table.SetBorder(new SolidBorder(BORDER_COLOR, 0.5f));

            // Header row
            var headers = new[] { "Product #", "Description", "Qty", "Unit Price", "Total", "HS Code" };
            foreach (var header in headers)
            {
                var headerCell = new Cell()
                    .SetHeight(15)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBackgroundColor(HEADER_BG)
                    .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                    .SetPadding(3)
                    .Add(new Paragraph(header)
                        .SetFont(PdfFontFactory.CreateFont(HELVETICA_BOLD))
                        .SetFontSize(7)
                        .SetFontColor(ColorConstants.WHITE)
                        .SetTextAlignment(TextAlignment.CENTER));
                table.AddHeaderCell(headerCell);
            }

            // Product rows
            if (invoice?.Products != null)
            {
                foreach (var product in invoice.Products)
                {
                    AddProductRow(table, product);
                }
            }

            // Total row
            AddTotalRow(table, invoice);

            table.SetWidth(UnitValue.CreatePercentValue(100));
            document.Add(table);
        }

        private void AddProductRow(Table table, ProductInformation product)
        {
            var rowData = new[] {
                product.ProductNumber ?? "-",
                product.ProductDescription ?? "-",
                product.Quantity.ToString(),
                product.UnitPrice.ToString("F2"),
                product.TotalAmount.ToString("F2"),
                product.HSCode ?? "-"
            };

            foreach (var data in rowData)
            {
                var isNumeric = data == rowData[2] || data == rowData[3] || data == rowData[4];
                var cell = new Cell()
                    .SetHeight(14)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBackgroundColor(ColorConstants.WHITE)
                    .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                    .SetPadding(3)
                    .Add(new Paragraph(data)
                        .SetFont(PdfFontFactory.CreateFont(HELVETICA))
                        .SetFontSize(7)
                        .SetFontColor(TEXT_BLACK)
                        .SetTextAlignment(isNumeric ? TextAlignment.RIGHT : TextAlignment.LEFT));
                table.AddCell(cell);
            }
        }

        private void AddTotalRow(Table table, CommercialInvoice invoice)
        {
            // Empty cells for first 4 columns
            for (int i = 0; i < 4; i++)
            {
                var emptyCell = new Cell()
                    .SetHeight(14)
                    .SetBackgroundColor(TOTAL_BG)
                    .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                    .SetPadding(0);
                table.AddCell(emptyCell);
            }

            // Total label
            var labelCell = new Cell()
                .SetHeight(14)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetBackgroundColor(TOTAL_BG)
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetPadding(3)
                .Add(new Paragraph("Total:")
                    .SetFont(PdfFontFactory.CreateFont(HELVETICA_BOLD))
                    .SetFontSize(7)
                    .SetFontColor(TEXT_BLACK)
                    .SetTextAlignment(TextAlignment.RIGHT));
            table.AddCell(labelCell);

            // Total amount
            var amountCell = new Cell()
                .SetHeight(14)
                .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                .SetBackgroundColor(TOTAL_BG)
                .SetBorder(new SolidBorder(BORDER_COLOR, 0.5f))
                .SetPadding(3)
                .Add(new Paragraph($"{invoice.Currency} {invoice.TotalAmount:F2}")
                    .SetFont(PdfFontFactory.CreateFont(HELVETICA_BOLD))
                    .SetFontSize(7)
                    .SetFontColor(TEXT_BLACK)
                    .SetTextAlignment(TextAlignment.RIGHT));
            table.AddCell(amountCell);
        }

        private void AddPaymentSection(Document document, CommercialInvoice invoice)
        {
            var table = new Table(new float[] { 515f });
            table.SetBorder(Border.NO_BORDER);

            var paymentData = new[] {
                $"Payment Term: {ValueOrDefault(invoice.InvoiceInfo?.PaymentTerm, DEFAULT_PAYMENT_TERM)}",
                $"Incoterms: {ValueOrDefault(invoice.InvoiceInfo?.Incoterms, DEFAULT_INCOTERMS)}",
                $"Country of Origin: {ValueOrDefault(invoice.InvoiceInfo?.CountryOfOrigin, DEFAULT_COUNTRY_OF_ORIGIN)}",
                "Bank Information:",
                "Oversea-Chinese Business Corporation Limited",
                "65 Chulia Street, #09-00 OCBC Centre, Singapore 049513",
                "A/C Name: Landi International (Singapore) Pte Ltd",
                "A/C No.: 517421103201(USD Current)",
                "SWIFT: OCBCSGSG"
            };

            foreach (var line in paymentData)
            {
                var cell = new Cell()
                    .SetHeight(10)
                    .SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .SetBorder(Border.NO_BORDER)
                    .SetPadding(2)
                    .Add(new Paragraph(line)
                        .SetFont(PdfFontFactory.CreateFont(HELVETICA))
                        .SetFontSize(7)
                        .SetFontColor(TEXT_BLACK));
                table.AddCell(cell);
            }

            table.SetWidth(UnitValue.CreatePercentValue(100));
            document.Add(table);

            // Footer
            document.Add(new Paragraph("This is a computer-generated document. No signature is required.")
                .SetFont(PdfFontFactory.CreateFont(HELVETICA))
                .SetFontSize(6)
                .SetFontColor(new DeviceGray(0.5f) as Color)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetHeight(8));
        }

        private static string ValueOrDefault(string? value, string defaultValue)
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value.Trim();
        }
    }
}


