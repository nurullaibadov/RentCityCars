using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RC.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Persistence.Configurations
{
    public class DriverConfiguration : IEntityTypeConfiguration<Driver>
    {
        public void Configure(EntityTypeBuilder<Driver> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.LicenseNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(d => d.PricePerDay)
                .HasPrecision(18, 2);

            builder.Property(d => d.PricePerHour)
                .HasPrecision(18, 2);

            // Languages conversion (List<string> -> CSV string)
            builder.Property(d => d.Languages)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            // User relationship (AppUser has one Driver)
            builder.HasOne(d => d.User)
                .WithOne(u => u.Driver) // AppUser.Driver tekil
                .HasForeignKey<Driver>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.AssignedBookings)
                .WithOne(b => b.AssignedDriver)
                .HasForeignKey(b => b.AssignedDriverId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(d => d.LicenseNumber).IsUnique();
            builder.HasIndex(d => d.Status);
            builder.HasIndex(d => d.IsVerified);
        }
    }
}
