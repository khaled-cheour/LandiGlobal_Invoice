using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using LandiGlobalTemplate.Models;

namespace LandiGlobalTemplate.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        // DbSets
        public DbSet<InvoiceHistory> InvoiceHistories { get; set; }
        public DbSet<InvoiceHistoryProduct> InvoiceHistoryProducts { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<Dashboard> Dashboards { get; set; }
        public DbSet<ProductCatalogItem> ProductCatalogItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // InvoiceHistory Configuration
            modelBuilder.Entity<InvoiceHistory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.InvoiceNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
                entity.Property(e => e.Currency).HasMaxLength(3);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.CreatedAt).ValueGeneratedOnAdd();

                // Index for performance
                entity.HasIndex(e => e.InvoiceNumber);
                entity.HasIndex(e => e.CustomerName);
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.Status);

                // One-to-Many relationship
                entity.HasMany(e => e.Products)
                    .WithOne(p => p.Invoice)
                    .HasForeignKey(p => p.InvoiceHistoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<InvoiceHistoryProduct>(entity =>
            {
                entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            });

            // AuditLog Configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Entity).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UserId).HasMaxLength(255);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.UserId);
            });

            // Dashboard Configuration
            modelBuilder.Entity<Dashboard>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalInvoices).HasDefaultValue(0);
                entity.Property(e => e.TotalRevenue).HasPrecision(18, 2).HasDefaultValue(0);
                entity.Property(e => e.AverageInvoiceValue).HasPrecision(18, 2);
                entity.Property(e => e.CurrentMonthRevenue).HasPrecision(18, 2);
                entity.Property(e => e.UpdatedAt).ValueGeneratedOnAddOrUpdate();
            });

            modelBuilder.Entity<ProductCatalogItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ProductNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.FamilyName).HasMaxLength(120);
                entity.Property(e => e.Platform).HasMaxLength(120);
                entity.Property(e => e.Model).IsRequired();
                entity.Property(e => e.MainDisplay).HasMaxLength(50);
                entity.Property(e => e.SecondDisplay).HasMaxLength(50);
                entity.Property(e => e.PaymentType).HasMaxLength(80);
                entity.Property(e => e.MemoryPlan).HasMaxLength(80);
                entity.Property(e => e.G4).HasMaxLength(40);
                entity.Property(e => e.GMS).HasMaxLength(40);
                entity.Property(e => e.HSCode).HasMaxLength(20).IsRequired(false);
                entity.Property(e => e.CreatedAt).ValueGeneratedOnAdd();
                entity.HasIndex(e => e.ProductNumber).IsUnique();
            });
        }
    }
}
