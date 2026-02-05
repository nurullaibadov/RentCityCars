using RC.Application.DTOs.Payment;
using RC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Application.DTOs.Booking
{
    public class CreateBookingRequestDto
    {
        public Guid CarId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Guid PickupLocationId { get; set; }
        public Guid ReturnLocationId { get; set; }
        public bool NeedsDriver { get; set; } = false;
        public bool WithInsurance { get; set; } = true;
        public bool WithGPS { get; set; } = false;
        public bool WithChildSeat { get; set; } = false;
        public int AdditionalDrivers { get; set; } = 0;
        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class BookingDto
    {
        public Guid Id { get; set; }
        public string BookingNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }
        public decimal TotalAmount { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Car Info
        public Guid CarId { get; set; }
        public string CarBrand { get; set; } = string.Empty;
        public string CarModel { get; set; } = string.Empty;
        public string CarImageUrl { get; set; } = string.Empty;

        // Locations
        public string PickupLocationName { get; set; } = string.Empty;
        public string ReturnLocationName { get; set; } = string.Empty;

        // Driver
        public string? DriverName { get; set; }

        // Payment
        public bool IsPaid { get; set; }
        public PaymentStatus? PaymentStatus { get; set; }
    }

    public class BookingDetailsDto : BookingDto
    {
        public decimal PricePerDay { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DepositAmount { get; set; }

        public bool WithInsurance { get; set; }
        public bool WithGPS { get; set; }
        public bool WithChildSeat { get; set; }
        public int AdditionalDrivers { get; set; }

        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string? Notes { get; set; }

        public DateTime? ActualPickupTime { get; set; }
        public DateTime? ActualReturnTime { get; set; }

        public PaymentDto? Payment { get; set; }
    }

    public class BookingCalculationDto
    {
        public int TotalDays { get; set; }
        public decimal PricePerDay { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DriverCost { get; set; }
        public decimal InsuranceCost { get; set; }
        public decimal GPSCost { get; set; }
        public decimal ChildSeatCost { get; set; }
        public decimal AdditionalDriversCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class UpdateBookingStatusRequestDto
    {
        public Guid BookingId { get; set; }
        public BookingStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}
