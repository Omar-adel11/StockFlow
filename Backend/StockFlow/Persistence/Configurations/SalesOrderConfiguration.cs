using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
    {
        public void Configure(EntityTypeBuilder<SalesOrder> entity)
        {
            entity.ToTable("SalesOrders");

            entity.HasKey(s => s.Id);

            entity.Property(s => s.OrderDate).IsRequired();
            entity.Property(s => s.CreatedByUserId).IsRequired();
            entity.Property(s => s.TotalAmount).HasColumnType("decimal(10,2)");

            entity.Property(s => s.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20)
                  .IsRequired();

            entity.HasOne(s => s.Customer)
                  .WithMany(c => c.SalesOrders)
                  .HasForeignKey(s => s.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(s => s.Warehouse)
                  .WithMany()
                  .HasForeignKey(s => s.WarehouseId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
