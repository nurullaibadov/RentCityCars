using RC.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Entities
{
    public class Review : BaseAuditableEntity
    {
        // User & Car
        public string UserId { get; set; } = string.Empty;
        public virtual AppUser User { get; set; } = null!;

        public Guid CarId { get; set; }
        public virtual Car Car { get; set; } = null!;

        public Guid? BookingId { get; set; }
        public virtual Booking? Booking { get; set; }

        // Rating (1-5 stars)
        public int Rating { get; set; }

        // Detailed Ratings
        public int? CleanlinessRating { get; set; }
        public int? ComfortRating { get; set; }
        public int? PerformanceRating { get; set; }
        public int? ValueForMoneyRating { get; set; }

        // Review Content
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;

        // Images
        public List<string> ImageUrls { get; set; } = new();

        // Verification
        public bool IsVerifiedRental { get; set; } = false;

        // Moderation
        public bool IsApproved { get; set; } = false;
        public bool IsReported { get; set; } = false;
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }

        // Helpfulness
        public int HelpfulCount { get; set; } = 0;
        public int NotHelpfulCount { get; set; } = 0;

        // Response from Company
        public string? CompanyResponse { get; set; }
        public DateTime? RespondedAt { get; set; }
    }
}
