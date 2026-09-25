using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configurations
{
    public class TeamInvitationConfiguration : IEntityTypeConfiguration<TeamInvitation>
    {
        public void Configure(EntityTypeBuilder<TeamInvitation> builder)
        {
            builder.HasKey(ti => ti.Id);

            builder.Property(ti => ti.Email)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.Property(ti => ti.Role)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(ti => ti.Token)
                   .IsRequired()
                   .HasMaxLength(256);

            // Ensure quick lookup by Token and prevent duplicate active tokens
            builder.HasIndex(ti => ti.Token)
                   .IsUnique();

            // Foreign Key to Business
            builder.HasOne(ti => ti.Business)
                   .WithMany()
                   .HasForeignKey(ti => ti.BusinessId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Foreign Key for InvitedByUserId (pointing to User entity)
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(ti => ti.InvitedByUserId)
                   .OnDelete(DeleteBehavior.Restrict); // Restrict deletion so deleting a user doesn't delete invitation audit logs

            // Ignore calculated property so EF Core doesn't attempt to map it as a database column
            builder.Ignore(ti => ti.IsActive);

            builder.HasIndex(e => e.BusinessId);
        }
    }
}