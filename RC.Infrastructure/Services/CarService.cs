using Microsoft.EntityFrameworkCore;
using RC.Application.DTOs.Car;
using RC.Application.DTOs.Common;
using RC.Application.Interfaces.Repositories;
using RC.Application.Interfaces.Services;
using RC.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Infrastructure.Services
{
    public class CarService : ICarService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CarService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<PagedResponse<CarDto>>> GetAllCarsAsync(CarSearchRequestDto request)
        {
            try
            {
                var query = _unitOfWork.Cars.GetQueryableNoTracking();

                // Apply filters
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    query = query.Where(c => c.Brand.Contains(request.SearchTerm) ||
                                            c.Model.Contains(request.SearchTerm) ||
                                            c.Category.Contains(request.SearchTerm));
                }

                if (!string.IsNullOrEmpty(request.Category))
                {
                    query = query.Where(c => c.Category == request.Category);
                }

                if (request.MinPrice.HasValue)
                {
                    query = query.Where(c => c.PricePerDay >= request.MinPrice.Value);
                }

                if (request.MaxPrice.HasValue)
                {
                    query = query.Where(c => c.PricePerDay <= request.MaxPrice.Value);
                }

                if (request.Transmission.HasValue)
                {
                    query = query.Where(c => c.Transmission == request.Transmission.Value);
                }

                if (request.FuelType.HasValue)
                {
                    query = query.Where(c => c.FuelType == request.FuelType.Value);
                }

                if (request.MinSeats.HasValue)
                {
                    query = query.Where(c => c.Seats >= request.MinSeats.Value);
                }

                // Check availability if dates provided
                if (request.StartDate.HasValue && request.EndDate.HasValue)
                {
                    var availableCars = await _unitOfWork.Cars.GetAvailableCarsAsync(
                        request.StartDate.Value,
                        request.EndDate.Value
                    );
                    var availableCarIds = availableCars.Select(c => c.Id).ToList();
                    query = query.Where(c => availableCarIds.Contains(c.Id));
                }

                // Get total count
                var totalCount = await query.CountAsync();

                // Apply pagination
                var cars = await query
                    .Include(c => c.Location)
                    .OrderBy(c => c.CreatedAt)
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var carDtos = cars.Select(MapToCarDto).ToList();

                var pagedResponse = new PagedResponse<CarDto>(carDtos, totalCount, request.PageNumber, request.PageSize);

                return ApiResponse<PagedResponse<CarDto>>.SuccessResponse(pagedResponse);
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<CarDto>>.ErrorResponse($"Error retrieving cars: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CarDetailsDto>> GetCarByIdAsync(Guid id)
        {
            try
            {
                var car = await _unitOfWork.Cars.GetQueryableNoTracking()
                    .Include(c => c.Location)
                    .Include(c => c.Reviews.Where(r => r.IsApproved))
                        .ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (car == null)
                {
                    return ApiResponse<CarDetailsDto>.ErrorResponse("Car not found");
                }

                var carDto = MapToCarDetailsDto(car);

                return ApiResponse<CarDetailsDto>.SuccessResponse(carDto);
            }
            catch (Exception ex)
            {
                return ApiResponse<CarDetailsDto>.ErrorResponse($"Error retrieving car: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<CarDto>>> GetFeaturedCarsAsync(int count = 10)
        {
            try
            {
                var cars = await _unitOfWork.Cars.GetFeaturedCarsAsync(count);
                var carDtos = cars.Select(MapToCarDto);

                return ApiResponse<IEnumerable<CarDto>>.SuccessResponse(carDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<CarDto>>.ErrorResponse($"Error retrieving featured cars: {ex.Message}");
            }
        }

        public async Task<ApiResponse<IEnumerable<CarDto>>> GetAvailableCarsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var cars = await _unitOfWork.Cars.GetAvailableCarsAsync(startDate, endDate);
                var carDtos = cars.Select(MapToCarDto);

                return ApiResponse<IEnumerable<CarDto>>.SuccessResponse(carDtos);
            }
            catch (Exception ex)
            {
                return ApiResponse<IEnumerable<CarDto>>.ErrorResponse($"Error retrieving available cars: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CarDto>> CreateCarAsync(CreateCarRequestDto request)
        {
            try
            {
                var car = new Car
                {
                    Brand = request.Brand,
                    Model = request.Model,
                    Year = request.Year,
                    Color = request.Color,
                    LicensePlate = request.LicensePlate,
                    VIN = request.VIN,
                    PricePerDay = request.PricePerDay,
                    PricePerWeek = request.PricePerDay * 7 * 0.9m, // 10% discount
                    PricePerMonth = request.PricePerDay * 30 * 0.8m, // 20% discount
                    DepositAmount = request.DepositAmount,
                    Category = request.Category,
                    Transmission = request.Transmission,
                    FuelType = request.FuelType,
                    Seats = request.Seats,
                    Doors = request.Doors,
                    LocationId = request.LocationId,
                    Description = request.Description,
                    AirConditioning = request.Features.AirConditioning,
                    GPS = request.Features.GPS,
                    Bluetooth = request.Features.Bluetooth,
                    ChildSeat = request.Features.ChildSeat,
                    USB = request.Features.USB,
                    SunRoof = request.Features.SunRoof,
                    LeatherSeats = request.Features.LeatherSeats,
                    CruiseControl = request.Features.CruiseControl,
                    ParkingSensors = request.Features.ParkingSensors,
                    ReverseCamera = request.Features.ReverseCamera
                };

                await _unitOfWork.Cars.AddAsync(car);
                await _unitOfWork.SaveChangesAsync();

                var carDto = MapToCarDto(car);

                return ApiResponse<CarDto>.SuccessResponse(carDto, "Car created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CarDto>.ErrorResponse($"Error creating car: {ex.Message}");
            }
        }

        public async Task<ApiResponse<CarDto>> UpdateCarAsync(UpdateCarRequestDto request)
        {
            try
            {
                var car = await _unitOfWork.Cars.GetByIdAsync(request.Id);
                if (car == null)
                {
                    return ApiResponse<CarDto>.ErrorResponse("Car not found");
                }

                car.Brand = request.Brand;
                car.Model = request.Model;
                car.Year = request.Year;
                car.Color = request.Color;
                car.PricePerDay = request.PricePerDay;
                car.Category = request.Category;
                car.Description = request.Description;

                await _unitOfWork.Cars.UpdateAsync(car);
                await _unitOfWork.SaveChangesAsync();

                var carDto = MapToCarDto(car);

                return ApiResponse<CarDto>.SuccessResponse(carDto, "Car updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<CarDto>.ErrorResponse($"Error updating car: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> DeleteCarAsync(Guid id)
        {
            try
            {
                await _unitOfWork.Cars.SoftDeleteAsync(id);
                await _unitOfWork.SaveChangesAsync();

                return ApiResponse<string>.SuccessResponse("", "Car deleted successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.ErrorResponse($"Error deleting car: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> CheckAvailabilityAsync(Guid carId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var isAvailable = await _unitOfWork.Cars.IsCarAvailableAsync(carId, startDate, endDate);
                return ApiResponse<bool>.SuccessResponse(isAvailable);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Error checking availability: {ex.Message}");
            }
        }

        private CarDto MapToCarDto(Car car)
        {
            return new CarDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                Color = car.Color,
                PricePerDay = car.PricePerDay,
                PricePerWeek = car.PricePerWeek,
                PricePerMonth = car.PricePerMonth,
                DiscountPercentage = car.DiscountPercentage,
                Category = car.Category,
                Transmission = car.Transmission,
                FuelType = car.FuelType,
                Seats = car.Seats,
                Doors = car.Doors,
                MainImageUrl = car.MainImageUrl,
                ImageUrls = car.ImageUrls,
                Status = car.Status,
                IsFeatured = car.IsFeatured,
                IsAvailableForBooking = car.IsAvailableForBooking,
                LocationName = car.Location?.Name,
                Features = new CarFeaturesDto
                {
                    AirConditioning = car.AirConditioning,
                    GPS = car.GPS,
                    Bluetooth = car.Bluetooth,
                    ChildSeat = car.ChildSeat,
                    USB = car.USB,
                    SunRoof = car.SunRoof,
                    LeatherSeats = car.LeatherSeats,
                    CruiseControl = car.CruiseControl,
                    ParkingSensors = car.ParkingSensors,
                    ReverseCamera = car.ReverseCamera
                }
            };
        }

        private CarDetailsDto MapToCarDetailsDto(Car car)
        {
            var dto = new CarDetailsDto
            {
                Id = car.Id,
                Brand = car.Brand,
                Model = car.Model,
                Year = car.Year,
                Color = car.Color,
                LicensePlate = car.LicensePlate,
                PricePerDay = car.PricePerDay,
                PricePerWeek = car.PricePerWeek,
                PricePerMonth = car.PricePerMonth,
                Category = car.Category,
                Transmission = car.Transmission,
                FuelType = car.FuelType,
                Seats = car.Seats,
                Doors = car.Doors,
                Mileage = car.Mileage,
                EngineSize = car.EngineSize,
                Description = car.Description,
                ShortDescription = car.ShortDescription,
                DepositAmount = car.DepositAmount,
                MainImageUrl = car.MainImageUrl,
                ImageUrls = car.ImageUrls,
                Status = car.Status,
                IsFeatured = car.IsFeatured,
                Features = new CarFeaturesDto
                {
                    AirConditioning = car.AirConditioning,
                    GPS = car.GPS,
                    Bluetooth = car.Bluetooth,
                    ChildSeat = car.ChildSeat,
                    USB = car.USB,
                    SunRoof = car.SunRoof,
                    LeatherSeats = car.LeatherSeats,
                    CruiseControl = car.CruiseControl,
                    ParkingSensors = car.ParkingSensors,
                    ReverseCamera = car.ReverseCamera
                },
                Reviews = car.Reviews.Select(r => new ReviewDto
                {
                    Id = r.Id,
                    UserName = $"{r.User.FirstName} {r.User.LastName}",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToList()
            };

            if (car.Reviews.Any())
            {
                dto.AverageRating = (decimal)car.Reviews.Average(r => r.Rating);
                dto.TotalReviews = car.Reviews.Count;
            }

            return dto;
        }
    }

}
