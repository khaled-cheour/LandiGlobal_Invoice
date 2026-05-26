namespace LandiGlobalTemplate.Models
{
    public class CustomerInformation
    {
        public string CustomerName { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string ShipToAddress { get; set; } = string.Empty;
        public string ShipFromAddress { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string VAT { get; set; } = string.Empty;
        public string EORI { get; set; } = string.Empty;
    }
}
