using RC.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Entities
{
    public class Settings : BaseAuditableEntity
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Category { get; set; } = "General"; // General, Payment, Email, etc.
        public bool IsPublic { get; set; } = false;
    }
}
