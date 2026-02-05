using RC.Domain.Common;
using RC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Entities
{
    public class Driver : BaseAuditableEntity
    {
        // User Reference
        public string UserId { get; set; } = string.Empty;
        public virtual AppUser User { get; set; } = null!;

        // License Information
        public string LicenseNumber { get; set; } = string.Empty;
        public DateTime LicenseIssueDate { get; set; }
        public DateTime LicenseExpiryDate { get; set; }
        public string LicenseCategory { get; set; } = string.Empty; // A, B, C, D, E
        public string? LicenseImageUrl { get; set; }

        // Personal Information
        public int YearsOfExperience { get; set; }
        public List<string> Languages { get; set; } = new(); // en, az, ru, tr

        // Status & Availability
        public DriverStatus Status { get; set; } = DriverStatus.Available;
        public bool IsApproved { get; set; } = false;
        public bool IsVerified { get; set; } = false;
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        // Rating
        public decimal AverageRating { get; set; } = 0;
        public int TotalRatings { get; set; } = 0;
        public int TotalTrips { get; set; } = 0;

        // Pricing
        public decimal PricePerDay { get; set; }
        public decimal PricePerHour { get; set; }

        // Current Location (for tracking)
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public DateTime? LastLocationUpdate { get; set; }

        // Documents
        public string? BackgroundCheckUrl { get; set; }
        public string? MedicalCertificateUrl { get; set; }
        public DateTime? BackgroundCheckDate { get; set; }
        public DateTime? MedicalCertificateDate { get; set; }

        // Emergency Contact
        public string? EmergencyContactName { get; set; }
        public string? EmergencyContactPhone { get; set; }

        // Bio
        public string? Bio { get; set; }
        public string? Specialties { get; set; } // Airport transfers, Wedding events, etc.

        // Navigation Properties
        public virtual ICollection<Booking> AssignedBookings { get; set; } = new List<Booking>();
    }

}
