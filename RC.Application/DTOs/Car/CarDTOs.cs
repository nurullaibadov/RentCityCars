using RC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Application.DTOs.Car
{
    public class CarDto
    {
        public Guid Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string Category { get; set; } = string.Empty;
        public TransmissionType Transmission { get; set; }
        public FuelType FuelType { get; set; }
        public int Seats { get; set; }
        public int Doors { get; set; }
        public string MainImageUrl { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new();
        public CarStatus Status { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsAvailableForBooking { get; set; }
        public string? LocationName { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public CarFeaturesDto Features { get; set; } = new();
    }

    public class CarFeaturesDto
    {
        public bool AirConditioning { get; set; }
        public bool GPS { get; set; }
        public bool Bluetooth { get; set; }
        public bool ChildSeat { get; set; }
        public bool USB { get; set; }
        public bool SunRoof { get; set; }
        public bool LeatherSeats { get; set; }
        public bool CruiseControl { get; set; }
        public bool ParkingSensors { get; set; }
        public bool ReverseCamera { get; set; }
    }

    public class CarDetailsDto : CarDto
    {
        public string LicensePlate { get; set; } = string.Empty;
        public int Mileage { get; set; }
        public decimal EngineSize { get; set; }
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public decimal DepositAmount { get; set; }
        public List<ReviewDto> Reviews { get; set; } = new();
    }

    public class CreateCarRequestDto
    {
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public decimal DepositAmount { get; set; }
        public string Category { get; set; } = string.Empty;
        public TransmissionType Transmission { get; set; }
        public FuelType FuelType { get; set; }
        public int Seats { get; set; }
        public int Doors { get; set; }
        public Guid? LocationId { get; set; }
        public string Description { get; set; } = string.Empty;
        public CarFeaturesDto Features { get; set; } = new();
    }

    public class UpdateCarRequestDto : CreateCarRequestDto
    {
        public Guid Id { get; set; }
    }

    public class CarSearchRequestDto
    {
        public string? SearchTerm { get; set; }
        public string? Category { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public TransmissionType? Transmission { get; set; }
        public FuelType? FuelType { get; set; }
        public int? MinSeats { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class ReviewDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
