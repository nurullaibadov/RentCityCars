using RC.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Entities
{
    public class FAQ : BaseAuditableEntity
    {
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;

        // Categorization
        public string Category { get; set; } = string.Empty; // Booking, Payment, General, etc.
        public int DisplayOrder { get; set; } = 0;

        // Status
        public bool IsActive { get; set; } = true;

        // Metadata
        public int ViewCount { get; set; } = 0;
        public bool IsFeatured { get; set; } = false;
    }

}
