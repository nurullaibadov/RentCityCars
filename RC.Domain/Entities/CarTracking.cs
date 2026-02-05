using RC.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Entities
{
    public class CarTracking : BaseEntity
    {
        public Guid CarId { get; set; }
        public virtual Car Car { get; set; } = null!;

        public Guid? BookingId { get; set; }
        public virtual Booking? Booking { get; set; }

        // GPS Coordinates
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double? Altitude { get; set; }

        // Movement Data
        public double? Speed { get; set; } // km/h
        public double? Heading { get; set; } // Degrees (0-360)

        // Timestamp
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Additional Data
        public double? BatteryLevel { get; set; }
        public int? OdometerReading { get; set; }
        public string? Status { get; set; } // Moving, Idle, Parked
    }
}
