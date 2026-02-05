using RC.Domain.Common;
using RC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public virtual AppUser User { get; set; } = null!;

        // Notification Details
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }

        // Status
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        // Link/Action
        public string? ActionUrl { get; set; }
        public string? ActionType { get; set; } // BookingUpdate, PaymentSuccess, etc.

        // Related Entities
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; } // Booking, Payment, etc.

        // Priority
        public bool IsImportant { get; set; } = false;

        // Metadata
        public string? Metadata { get; set; } // JSON format for additional data
    }

}
