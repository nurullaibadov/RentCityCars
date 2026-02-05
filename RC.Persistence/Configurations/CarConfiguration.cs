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
    public class CarConfiguration : IEntityTypeConfiguration<Car>
    {
        public void Configure(EntityTypeBuilder<Car> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Brand)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(c => c.Model)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.LicensePlate)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.VIN)
                .IsRequired()
                .HasMaxLength(17);

            builder.Property(c => c.PricePerDay).HasPrecision(18, 2);
            builder.Property(c => c.PricePerWeek).HasPrecision(18, 2);
            builder.Property(c => c.PricePerMonth).HasPrecision(18, 2);
            builder.Property(c => c.DepositAmount).HasPrecision(18, 2);
            builder.Property(c => c.DiscountPercentage).HasPrecision(5, 2);
            builder.Property(c => c.EngineSize).HasPrecision(5, 2);

            // ImageUrls -> List<string> converter
            builder.Property(c => c.ImageUrls)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            // Relations
            builder.HasOne(c => c.Location)
                .WithMany(l => l.Cars)
                .HasForeignKey(c => c.LocationId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(c => c.Insurance)
                .WithOne(i => i.Car)
                .HasForeignKey<Insurance>(i => i.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Bookings)
                .WithOne(b => b.Car)
                .HasForeignKey(b => b.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Reviews)
                .WithOne(r => r.Car)
                .HasForeignKey(r => r.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(c => c.LicensePlate).IsUnique();
            builder.HasIndex(c => c.VIN).IsUnique();
            builder.HasIndex(c => c.Status);
            builder.HasIndex(c => c.Category);
        }
    }
}