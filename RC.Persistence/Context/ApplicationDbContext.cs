using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RC.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Persistence.Context
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Car> Cars { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<CarTracking> CarTrackings { get; set; }
        public DbSet<Insurance> Insurances { get; set; }
        public DbSet<Maintenance> Maintenances { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<Settings> Settings { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Global query filter for soft delete
            modelBuilder.Entity<Car>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Booking>().HasQueryFilter(b => !b.IsDeleted);
            modelBuilder.Entity<Driver>().HasQueryFilter(d => !d.IsDeleted);
            modelBuilder.Entity<Location>().HasQueryFilter(l => !l.IsDeleted);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added && entry.Entity is Domain.Common.BaseEntity addedEntity)
                {
                    addedEntity.CreatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Modified && entry.Entity is Domain.Common.BaseEntity modifiedEntity)
                {
                    modifiedEntity.UpdatedAt = DateTime.UtcNow;
                }

                if (entry.State == EntityState.Deleted && entry.Entity is Domain.Common.BaseEntity deletedEntity)
                {
                    entry.State = EntityState.Modified;
                    deletedEntity.IsDeleted = true;
                    deletedEntity.DeletedAt = DateTime.UtcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
