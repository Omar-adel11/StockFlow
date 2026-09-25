using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> entity)
        {
            entity.ToTable("Products");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
            entity.Property(p => p.SKU).IsRequired().HasMaxLength(50);
            entity.Property(p => p.UnitPrice).HasColumnType("decimal(10,2)");
            entity.Property(p => p.ReorderLevel).IsRequired();
            entity.Property(p => p.IsActive).IsRequired();

            // SKU must be unique across the whole catalog (per-tenant
            // uniqueness instead, once multi-tenancy is added later).
            entity.HasIndex(p => p.SKU).IsUnique();

            // Restrict: a category with products can't be deleted out
            // from under them - the category must be reassigned or the
            // products soft-deleted first.
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            // SetNull: losing a preferred supplier shouldn't take the
            // product down with it - PreferredSupplierId is nullable for
            // exactly this reason.
            entity.HasOne(p => p.PreferredSupplier)
                  .WithMany(s => s.PreferredByProducts)
                  .HasForeignKey(p => p.PreferredSupplierId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(c => c.Business).WithMany().HasForeignKey(c => c.BusinessId).OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => new { p.BusinessId, p.SKU }).IsUnique();
            entity.HasIndex(p => new { p.BusinessId, p.IsActive }).IsUnique();

            entity.Property(p => p.UnitPrice)
   .HasPrecision(18, 2);
        }
    }
}
