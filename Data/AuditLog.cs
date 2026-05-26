namespace LandiGlobalTemplate.Data
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;  // Create, Update, Delete
        public string Entity { get; set; } = string.Empty;  // InvoiceHistory, Product, etc.
        public int? EntityId { get; set; }
        public string? OldValues { get; set; }  // JSON
        public string? NewValues { get; set; }  // JSON
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? UserId { get; set; }
    }

    public class Dashboard
    {
        public int Id { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalRevenue { get; set; }
        public int ProcessedCount { get; set; }
        public int DraftCount { get; set; }
        public int ErrorCount { get; set; }
        public decimal AverageInvoiceValue { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Monthly statistics
        public int? CurrentMonthInvoices { get; set; }
        public decimal? CurrentMonthRevenue { get; set; }
        
        // Top customers
        public string? TopCustomer { get; set; }
        public int? TopCustomerInvoiceCount { get; set; }
    }
}
