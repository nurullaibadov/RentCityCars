using RC.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty; // Created, Updated, Deleted, etc.
        public string EntityName { get; set; } = string.Empty; // Car, Booking, etc.
        public string EntityId { get; set; } = string.Empty;

        public string? OldValues { get; set; } // JSON
        public string? NewValues { get; set; } // JSON

        public string IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

}
