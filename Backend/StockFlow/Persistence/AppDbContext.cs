using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
        public DbSet<Plan> Plans => Set<Plan>();
        public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.ToTable("ContactMessages");

                entity.HasKey(c => c.Id);

                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Email).IsRequired().HasMaxLength(200);
                entity.Property(c => c.Subject).IsRequired().HasMaxLength(200);
                entity.Property(c => c.Message).IsRequired().HasMaxLength(2000);
                entity.Property(c => c.SubmittedAtUtc).IsRequired();
            });

            modelBuilder.Entity<Plan>(entity =>
            {
                entity.ToTable("Plans");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Price).HasColumnType("decimal(10,2)");
                entity.Property(p => p.Description).HasMaxLength(500);
                entity.Property(p => p.IsActive).IsRequired();

                // Stores the enum as its text name ("Monthly") instead of an
                // int (0/1) in the DB - a lot easier to read a row and know
                // what it means without cross-referencing the enum in code.
                entity.Property(p => p.BillingCycle)
                      .HasConversion<string>()
                      .HasMaxLength(20)
                      .IsRequired();

                // One Plan has many PlanFeatures. Cascade delete: removing
                // a Plan also removes its feature rows, so you never end up
                entity.HasMany(p => p.Features)
                      .WithOne(f => f.Plan)
                      .HasForeignKey(f => f.PlanId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PlanFeature>(entity =>
            {
                entity.ToTable("PlanFeatures");

                entity.HasKey(f => f.Id);

                entity.Property(f => f.Name).IsRequired().HasMaxLength(200);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
