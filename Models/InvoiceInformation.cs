namespace LandiGlobalTemplate.Models
{
    public class InvoiceInformation
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string InvoiceDate { get; set; } = string.Empty;
        public string PurchaseOrderNumber { get; set; } = string.Empty;
        public string PaymentTerm { get; set; } = string.Empty;
        public string Incoterms { get; set; } = string.Empty;
        public string CountryOfOrigin { get; set; } = string.Empty;
    }
}
