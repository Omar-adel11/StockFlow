using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
    {
        public void Configure(EntityTypeBuilder<InventoryItem> entity)
        {
            entity.ToTable("InventoryItems");

            entity.HasKey(i => i.Id);

            entity.Property(i => i.QuantityOnHand).IsRequired();

            // A product can only have one stock row per warehouse - this
            // is what makes "the" InventoryItem for a given
            // (ProductId, WarehouseId) lookup unambiguous.
            entity.HasIndex(i => new { i.ProductId, i.WarehouseId }).IsUnique();

            entity.HasOne(i => i.Product)
                  .WithMany(p => p.InventoryItems)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.Warehouse)
                  .WithMany(w => w.InventoryItems)
                  .HasForeignKey(i => i.WarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
