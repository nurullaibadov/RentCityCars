using RC.Domain.Common;
using RC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Entities
{
    public class Maintenance : BaseAuditableEntity
    {
        public Guid CarId { get; set; }
        public virtual Car Car { get; set; } = null!;

        // Maintenance Details
        public MaintenanceType Type { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Dates
        public DateTime ScheduledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? NextMaintenanceDate { get; set; }

        // Cost
        public decimal Cost { get; set; }
        public string? InvoiceNumber { get; set; }

        // Service Provider
        public string ServiceProvider { get; set; } = string.Empty;
        public string? ServiceProviderPhone { get; set; }

        // Status
        public bool IsCompleted { get; set; } = false;
        public bool IsUrgent { get; set; } = false;

        // Mileage
        public int MileageAtService { get; set; }

        // Parts Replaced
        public string? PartsReplaced { get; set; }

        // Documents
        public List<string> DocumentUrls { get; set; } = new();

        // Notes
        public string? Notes { get; set; }
    }
}
