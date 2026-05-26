namespace LandiGlobalTemplate.Data
{
    /// <summary>
    /// Historique persistant des factures traitées
    /// </summary>
    public class InvoiceHistory
    {
        public int Id { get; set; }
        
        // Invoice Info
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PurchaseOrderNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public string PaymentTerm { get; set; } = string.Empty;
        public string Incoterms { get; set; } = string.Empty;
        public string CountryOfOrigin { get; set; } = string.Empty;
        
        // Customer Info
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string ShipToAddress { get; set; } = string.Empty;
        public string ShipFromAddress { get; set; } = string.Empty;
        public string VAT { get; set; } = string.Empty;
        public string EORI { get; set; } = string.Empty;
        
        // Totals
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public int ProductCount { get; set; }
        
        // System Fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public string Status { get; set; } = "Processed"; // Processed, Draft, Error
        public string? ErrorMessage { get; set; }
        
        // Validation Status
        public bool IsValidated { get; set; } = false;
        public bool IsEnriched { get; set; } = false;
        public int InvalidProductCount { get; set; }
        
        // Navigation
        public virtual ICollection<InvoiceHistoryProduct> Products { get; set; } = new List<InvoiceHistoryProduct>();
        
        // PDF Storage
        public byte[]? OriginalPDF { get; set; }
        public byte[]? EnrichedPDF { get; set; }
    }

    /// <summary>
    /// Détail des produits dans l'historique des factures
    /// </summary>
    public class InvoiceHistoryProduct
    {
        public int Id { get; set; }
        
        // Foreign Key
        public int InvoiceHistoryId { get; set; }
        
        // Product Info
        public string ProductNumber { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        
        // Display Info
        public string MainDisplay { get; set; } = string.Empty;
        public string SecondDisplay { get; set; } = string.Empty;
        
        // Quantity & Pricing
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalAmount { get; set; }
        
        // Additional Info
        public string HSCode { get; set; } = string.Empty;
        public string PaymentType { get; set; } = string.Empty;
        public string MemoryPlan { get; set; } = string.Empty;
        public string G4 { get; set; } = string.Empty;
        public string GMS { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        
        // Validation
        public bool IsValid { get; set; } = true;
        public bool IsEnriched { get; set; } = false;
        
        // Navigation
        public virtual InvoiceHistory Invoice { get; set; } = null!;
    }
}
