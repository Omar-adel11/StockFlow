using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
    {
        public void Configure(EntityTypeBuilder<StockMovement> entity)
        {
            entity.ToTable("StockMovements");

            entity.HasKey(m => m.Id);

            entity.Property(m => m.ChangeQuantity).IsRequired();
            entity.Property(m => m.CreatedAtUtc).IsRequired();

            entity.Property(m => m.Reason)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();
            entity.Property(s => s.Reason)
                  .HasConversion<string>();
            entity.HasOne(m => m.Product)
                  .WithMany(p => p.StockMovements)
                  .HasForeignKey(m => m.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Warehouse)
                  .WithMany()
                  .HasForeignKey(m => m.WarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Business).WithMany().HasForeignKey(c => c.BusinessId).OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.BusinessId);
        }
    }
}
