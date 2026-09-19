using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrder> entity)
        {
            entity.ToTable("PurchaseOrders");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.OrderDate).IsRequired();
            entity.Property(p => p.CreatedByUserId).IsRequired();
            entity.Property(p => p.TotalAmount).HasColumnType("decimal(10,2)");

            entity.Property(p => p.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();

            // Restrict: a supplier with purchase order history can't be
            // hard-deleted - soft-delete it (IsActive = false) instead,
            // so historical orders keep resolving to a real supplier row.
            entity.HasOne(p => p.Supplier)
                  .WithMany(s => s.PurchaseOrders)
                  .HasForeignKey(p => p.SupplierId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
