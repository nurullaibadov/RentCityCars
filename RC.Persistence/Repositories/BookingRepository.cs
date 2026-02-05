using Microsoft.EntityFrameworkCore;
using RC.Application.Interfaces.Repositories;
using RC.Domain.Entities;
using RC.Domain.Enums;
using RC.Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Persistence.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Booking>> GetUserBookingsAsync(string userId)
        {
            return await _dbSet
                .Where(b => b.UserId == userId)
                .Include(b => b.Car)
                .Include(b => b.PickupLocation)
                .Include(b => b.ReturnLocation)
                .Include(b => b.Payment)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByStatusAsync(BookingStatus status)
        {
            return await _dbSet
                .Where(b => b.Status == status)
                .Include(b => b.Car)
                .Include(b => b.User)
                .Include(b => b.PickupLocation)
                .Include(b => b.ReturnLocation)
                .ToListAsync();
        }

        public async Task<Booking?> GetBookingByNumberAsync(string bookingNumber)
        {
            return await _dbSet
                .Include(b => b.Car)
                .Include(b => b.User)
                .Include(b => b.PickupLocation)
                .Include(b => b.ReturnLocation)
                .Include(b => b.Payment)
                .Include(b => b.AssignedDriver)
                .FirstOrDefaultAsync(b => b.BookingNumber == bookingNumber);
        }

        public async Task<IEnumerable<Booking>> GetUpcomingBookingsAsync()
        {
            return await _dbSet
                .Where(b => b.StartDate > DateTime.UtcNow && b.Status == BookingStatus.Confirmed)
                .Include(b => b.Car)
                .Include(b => b.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetActiveBookingsAsync()
        {
            var today = DateTime.UtcNow.Date;
            return await _dbSet
                .Where(b => b.StartDate <= today &&
                           b.EndDate >= today &&
                           b.Status == BookingStatus.InProgress)
                .Include(b => b.Car)
                .Include(b => b.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetBookingsByCarIdAsync(Guid carId)
        {
            return await _dbSet
                .Where(b => b.CarId == carId)
                .Include(b => b.User)
                .OrderByDescending(b => b.StartDate)
                .ToListAsync();
        }

        public async Task<bool> HasOverlappingBookingsAsync(Guid carId, DateTime startDate, DateTime endDate, Guid? excludeBookingId = null)
        {
            var query = _dbSet.Where(b => b.CarId == carId &&
                                         b.Status != BookingStatus.Cancelled &&
                                         ((b.StartDate <= startDate && b.EndDate >= startDate) ||
                                          (b.StartDate <= endDate && b.EndDate >= endDate) ||
                                          (b.StartDate >= startDate && b.EndDate <= endDate)));

            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.Id != excludeBookingId.Value);
            }

            return await query.AnyAsync();
        }
    }

}
