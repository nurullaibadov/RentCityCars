using RC.Domain.Common;
using RC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Entities
{
    public class Booking : BaseAuditableEntity
    {
        // User Information
        public string UserId { get; set; } = string.Empty;
        public virtual AppUser User { get; set; } = null!;

        // Car Information
        public Guid CarId { get; set; }
        public virtual Car Car { get; set; } = null!;

        // Booking Details
        public string BookingNumber { get; set; } = string.Empty; // Unique tracking number
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalDays { get; set; }

        // Pricing
        public decimal PricePerDay { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal DepositAmount { get; set; }
        public decimal TotalAmount { get; set; }

        // Pickup & Return
        public Guid PickupLocationId { get; set; }
        public virtual Location PickupLocation { get; set; } = null!;

        public Guid ReturnLocationId { get; set; }
        public virtual Location ReturnLocation { get; set; } = null!;

        public DateTime? ActualPickupTime { get; set; }
        public DateTime? ActualReturnTime { get; set; }

        // Driver Assignment
        public Guid? AssignedDriverId { get; set; }
        public virtual Driver? AssignedDriver { get; set; }
        public bool NeedsDriver { get; set; } = false;

        // Additional Options
        public bool WithInsurance { get; set; } = true;
        public bool WithGPS { get; set; } = false;
        public bool WithChildSeat { get; set; } = false;
        public int AdditionalDrivers { get; set; } = 0;

        // Status
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public string? Notes { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime? CancelledAt { get; set; }

        // Contact Information
        public string ContactPhone { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;

        // Navigation Properties
        public virtual Payment? Payment { get; set; }
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
