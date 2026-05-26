using Microsoft.AspNetCore.Identity;
using LandiGlobalTemplate.Data;

namespace LandiGlobalTemplate.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Department { get; set; }
        public string? Phone { get; set; }

        // Relationship
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
}
