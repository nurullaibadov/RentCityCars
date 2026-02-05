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
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.BookingNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(b => b.SubTotal).HasPrecision(18, 2);
            builder.Property(b => b.TaxAmount).HasPrecision(18, 2);
            builder.Property(b => b.DepositAmount).HasPrecision(18, 2);
            builder.Property(b => b.TotalAmount).HasPrecision(18, 2);

            // Relations
            builder.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Car)
                .WithMany(c => c.Bookings)
                .HasForeignKey(b => b.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.PickupLocation)
                .WithMany(l => l.PickupBookings)
                .HasForeignKey(b => b.PickupLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.ReturnLocation)
                .WithMany(l => l.ReturnBookings)
                .HasForeignKey(b => b.ReturnLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.AssignedDriver)
                .WithMany(d => d.AssignedBookings)
                .HasForeignKey(b => b.AssignedDriverId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(b => b.Payment)
                .WithOne(p => p.Booking)
                .HasForeignKey<Payment>(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(b => b.BookingNumber).IsUnique();
            builder.HasIndex(b => b.Status);
            builder.HasIndex(b => b.StartDate);
            builder.HasIndex(b => new { b.UserId, b.Status });
        }
    }

}
