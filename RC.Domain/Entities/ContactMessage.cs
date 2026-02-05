using RC.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Entities
{
    public class ContactMessage : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Status
        public bool IsRead { get; set; } = false;
        public bool IsReplied { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public DateTime? RepliedAt { get; set; }

        // Reply
        public string? ReplyMessage { get; set; }
        public string? RepliedBy { get; set; }

        // Category
        public string? Category { get; set; } // General, Booking, Complaint, etc.

        // User Reference (if logged in)
        public string? UserId { get; set; }
    }
}
