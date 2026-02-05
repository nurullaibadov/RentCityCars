using Microsoft.EntityFrameworkCore.Storage;
using RC.Application.Interfaces.Repositories;
using RC.Domain.Entities;
using RC.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        // Lazy initialization for repositories
        private ICarRepository? _cars;
        private IBookingRepository? _bookings;
        private IPaymentRepository? _payments;
        private IDriverRepository? _drivers;
        private IReviewRepository? _reviews;
        private ILocationRepository? _locations;
        private INotificationRepository? _notifications;

        private IGenericRepository<Insurance>? _insurances;
        private IGenericRepository<Maintenance>? _maintenances;
        private IGenericRepository<CarTracking>? _carTrackings;
        private IGenericRepository<ContactMessage>? _contactMessages;
        private IGenericRepository<FAQ>? _faqs;
        private IGenericRepository<Settings>? _settings;
        private IGenericRepository<AuditLog>? _auditLogs;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public ICarRepository Cars => _cars ??= new CarRepository(_context);
        public IBookingRepository Bookings => _bookings ??= new BookingRepository(_context);
        public IPaymentRepository Payments => _payments ??= new PaymentRepository(_context);
        public IDriverRepository Drivers => _drivers ??= new DriverRepository(_context);
        public IReviewRepository Reviews => _reviews ??= new ReviewRepository(_context);
        public ILocationRepository Locations => _locations ??= new LocationRepository(_context);
        public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_context);

        public IGenericRepository<Insurance> Insurances => _insurances ??= new GenericRepository<Insurance>(_context);
        public IGenericRepository<Maintenance> Maintenances => _maintenances ??= new GenericRepository<Maintenance>(_context);
        public IGenericRepository<CarTracking> CarTrackings => _carTrackings ??= new GenericRepository<CarTracking>(_context);
        public IGenericRepository<ContactMessage> ContactMessages => _contactMessages ??= new GenericRepository<ContactMessage>(_context);
        public IGenericRepository<FAQ> FAQs => _faqs ??= new GenericRepository<FAQ>(_context);
        public IGenericRepository<Settings> Settings => _settings ??= new GenericRepository<Settings>(_context);
        public IGenericRepository<AuditLog> AuditLogs => _auditLogs ??= new GenericRepository<AuditLog>(_context);

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }

}
