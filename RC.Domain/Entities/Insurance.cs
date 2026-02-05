using RC.Domain.Common;
using RC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Entities
{
    public class Insurance : BaseAuditableEntity
    {
        public Guid CarId { get; set; }
        public virtual Car Car { get; set; } = null!;

        // Insurance Details
        public string PolicyNumber { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public InsuranceType Type { get; set; }

        // Coverage
        public decimal CoverageAmount { get; set; }
        public decimal DeductibleAmount { get; set; }
        public decimal PremiumAmount { get; set; }

        // Dates
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Documents
        public string? PolicyDocumentUrl { get; set; }

        // Coverage Details
        public string CoverageDetails { get; set; } = string.Empty;
        public string? Exclusions { get; set; }

        // Contact
        public string? ProviderPhone { get; set; }
        public string? ProviderEmail { get; set; }
    }

}
