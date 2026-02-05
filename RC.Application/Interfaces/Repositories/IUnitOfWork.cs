using RC.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Application.Interfaces.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        ICarRepository Cars { get; }
        IBookingRepository Bookings { get; }
        IPaymentRepository Payments { get; }
        IDriverRepository Drivers { get; }
        IReviewRepository Reviews { get; }
        ILocationRepository Locations { get; }
        INotificationRepository Notifications { get; }

        IGenericRepository<Insurance> Insurances { get; }
        IGenericRepository<Maintenance> Maintenances { get; }
        IGenericRepository<CarTracking> CarTrackings { get; }
        IGenericRepository<ContactMessage> ContactMessages { get; }
        IGenericRepository<FAQ> FAQs { get; }
        IGenericRepository<Settings> Settings { get; }
        IGenericRepository<AuditLog> AuditLogs { get; }

        // Transactions
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
