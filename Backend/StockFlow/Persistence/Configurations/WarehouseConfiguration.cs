using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
    {
        public void Configure(EntityTypeBuilder<Warehouse> entity)
        {
            entity.ToTable("Warehouses");

            entity.HasKey(w => w.Id);

            entity.Property(w => w.Name).IsRequired().HasMaxLength(150);
            entity.Property(w => w.Location).HasMaxLength(300);
            entity.Property(w => w.IsActive).IsRequired();
        }
    }
}
