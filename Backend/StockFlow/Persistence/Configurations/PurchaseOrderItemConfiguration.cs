using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderItem> entity)
        {
            entity.ToTable("PurchaseOrderItems");

            entity.HasKey(i => i.Id);

            entity.Property(i => i.Quantity).IsRequired();
            entity.Property(i => i.UnitCost).HasColumnType("decimal(10,2)");

            // Cascade here (unlike Product/Supplier/Warehouse elsewhere):
            // a line item has no meaning without its parent order, so
            // deleting the whole PurchaseOrder should delete its items too.
            entity.HasOne(i => i.PurchaseOrder)
                  .WithMany(p => p.Items)
                  .HasForeignKey(i => i.PurchaseOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Product)
                  .WithMany(p => p.PurchaseOrderItems)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
