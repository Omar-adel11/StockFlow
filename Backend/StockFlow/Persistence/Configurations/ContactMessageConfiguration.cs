using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
    {
        public void Configure(EntityTypeBuilder<ContactMessage> entity)
        {
            entity.ToTable("ContactMessages");

            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Email).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Subject).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Message).IsRequired().HasMaxLength(2000);
            entity.Property(c => c.SubmittedAtUtc).IsRequired();
        }
    }
}
