using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class SalesOrderItemConfiguration : IEntityTypeConfiguration<SalesOrderItem>
    {
        public void Configure(EntityTypeBuilder<SalesOrderItem> entity)
        {
            entity.ToTable("SalesOrderItems");

            entity.HasKey(i => i.Id);

            entity.Property(i => i.Quantity).IsRequired();
            entity.Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");

            entity.HasOne(i => i.SalesOrder)
                  .WithMany(s => s.Items)
                  .HasForeignKey(i => i.SalesOrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Product)
                  .WithMany(p => p.SalesOrderItems)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);


            entity.HasOne(c => c.Business).WithMany().HasForeignKey(c => c.BusinessId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.BusinessId);
            entity.Property(p => p.UnitPrice)
   .HasPrecision(18, 2);
        }
    }
}
