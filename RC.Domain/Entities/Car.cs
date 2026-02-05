using RC.Domain.Common;
using RC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Entities
{
    public class Car : BaseAuditableEntity
    {
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Color { get; set; } = string.Empty;
        public string LicensePlate { get; set; } = string.Empty;
        public string VIN { get; set; } = string.Empty; // Vehicle Identification Number

        // Pricing
        public decimal PricePerDay { get; set; }
        public decimal PricePerWeek { get; set; }
        public decimal PricePerMonth { get; set; }
        public decimal DiscountPercentage { get; set; } = 0;
        public decimal DepositAmount { get; set; }

        // Specifications
        public TransmissionType Transmission { get; set; }
        public FuelType FuelType { get; set; }
        public int Seats { get; set; }
        public int Doors { get; set; }
        public decimal EngineSize { get; set; } // Liters
        public int Mileage { get; set; } // Current KM
        public string Category { get; set; } = string.Empty; // Economy, Luxury, SUV, etc.

        // Features
        public bool AirConditioning { get; set; } = true;
        public bool GPS { get; set; } = false;
        public bool Bluetooth { get; set; } = true;
        public bool ChildSeat { get; set; } = false;
        public bool USB { get; set; } = true;
        public bool AuxInput { get; set; } = true;
        public bool SunRoof { get; set; } = false;
        public bool LeatherSeats { get; set; } = false;
        public bool CruiseControl { get; set; } = false;
        public bool ParkingSensors { get; set; } = false;
        public bool ReverseCamera { get; set; } = false;
        public bool HeatedSeats { get; set; } = false;

        // Status & Availability
        public CarStatus Status { get; set; } = CarStatus.Available;
        public bool IsFeatured { get; set; } = false;
        public bool IsAvailableForBooking { get; set; } = true;
        public int AvailableQuantity { get; set; } = 1;

        // Location
        public Guid? LocationId { get; set; }
        public virtual Location? Location { get; set; }

        // Images
        public string MainImageUrl { get; set; } = string.Empty;
        public List<string> ImageUrls { get; set; } = new();

        // Description
        public string Description { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;

        // Insurance & Maintenance
        public Guid? InsuranceId { get; set; }
        public virtual Insurance? Insurance { get; set; }

        // Navigation Properties
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<CarTracking> TrackingHistory { get; set; } = new List<CarTracking>();
        public virtual ICollection<Maintenance> MaintenanceRecords { get; set; } = new List<Maintenance>();
    }
}
