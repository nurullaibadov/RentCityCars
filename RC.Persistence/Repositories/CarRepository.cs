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
    public class CarRepository : GenericRepository<Car>, ICarRepository
    {
        public CarRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Car>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate)
        {
            var bookedCarIds = await _context.Bookings
                .Where(b => b.Status != BookingStatus.Cancelled &&
                           ((b.StartDate <= startDate && b.EndDate >= startDate) ||
                            (b.StartDate <= endDate && b.EndDate >= endDate) ||
                            (b.StartDate >= startDate && b.EndDate <= endDate)))
                .Select(b => b.CarId)
                .ToListAsync();

            return await _dbSet
                .Where(c => c.Status == CarStatus.Available &&
                           c.IsAvailableForBooking &&
                           !bookedCarIds.Contains(c.Id))
                .Include(c => c.Location)
                .ToListAsync();
        }

        public async Task<IEnumerable<Car>> GetFeaturedCarsAsync(int count = 10)
        {
            return await _dbSet
                .Where(c => c.IsFeatured && c.IsAvailableForBooking && c.Status == CarStatus.Available)
                .Include(c => c.Location)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Car>> SearchCarsAsync(string searchTerm)
        {
            return await _dbSet
                .Where(c => c.Brand.Contains(searchTerm) ||
                           c.Model.Contains(searchTerm) ||
                           c.Category.Contains(searchTerm))
                .Include(c => c.Location)
                .ToListAsync();
        }

        public async Task<IEnumerable<Car>> GetCarsByCategoryAsync(string category)
        {
            return await _dbSet
                .Where(c => c.Category == category && c.IsAvailableForBooking)
                .Include(c => c.Location)
                .ToListAsync();
        }

        public async Task<IEnumerable<Car>> GetCarsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _dbSet
                .Where(c => c.PricePerDay >= minPrice && c.PricePerDay <= maxPrice && c.IsAvailableForBooking)
                .Include(c => c.Location)
                .ToListAsync();
        }

        public async Task<bool> IsCarAvailableAsync(Guid carId, DateTime startDate, DateTime endDate)
        {
            var hasOverlappingBooking = await _context.Bookings
                .AnyAsync(b => b.CarId == carId &&
                              b.Status != BookingStatus.Cancelled &&
                              ((b.StartDate <= startDate && b.EndDate >= startDate) ||
                               (b.StartDate <= endDate && b.EndDate >= endDate) ||
                               (b.StartDate >= startDate && b.EndDate <= endDate)));

            return !hasOverlappingBooking;
        }

        public async Task UpdateCarStatusAsync(Guid carId, CarStatus status)
        {
            var car = await GetByIdAsync(carId);
            if (car != null)
            {
                car.Status = status;
                await UpdateAsync(car);
            }
        }
    }

}
