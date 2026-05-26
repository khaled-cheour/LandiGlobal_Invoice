namespace LandiGlobalTemplate.Models
{
    public class ProductInformation
    {
        public string ProductNumber { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string MainDisplay { get; set; } = string.Empty;
        public string SecondDisplay { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        public string HSCode { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string MemoryPlan { get; set; } = string.Empty;
        public string G4 { get; set; } = string.Empty;
        public string GMS { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
    }
}
