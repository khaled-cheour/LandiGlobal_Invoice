using System.ComponentModel.DataAnnotations;

namespace LandiGlobalTemplate.Data
{
    public class ProductCatalogItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductNumber { get; set; } = string.Empty;

        [StringLength(120)]
        public string FamilyName { get; set; } = string.Empty;

        [StringLength(120)]
        public string Platform { get; set; } = string.Empty;

        [Required]
        public string Model { get; set; } = string.Empty;

        [StringLength(50)]
        public string MainDisplay { get; set; } = string.Empty;

        [StringLength(50)]
        public string SecondDisplay { get; set; } = string.Empty;

        [StringLength(80)]
        public string PaymentType { get; set; } = string.Empty;

        [StringLength(80)]
        public string MemoryPlan { get; set; } = string.Empty;

        [StringLength(40)]
        public string G4 { get; set; } = string.Empty;

        [StringLength(40)]
        public string GMS { get; set; } = string.Empty;

        [StringLength(20)]
        public string? HSCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
