namespace LandiGlobalTemplate.Models
{
    public class CommercialInvoice
    {
        public InvoiceInformation? InvoiceInfo { get; set; }
        public CustomerInformation? CustomerInfo { get; set; }
        public List<ProductInformation> Products { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public int PdfProductCount { get; set; }
        public decimal? PdfTotalAmount { get; set; }
        public List<string> ExtractionErrors { get; set; } = new();
        public string Currency { get; set; } = "EUR";
    }
}
