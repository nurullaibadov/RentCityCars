using RC.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Entities
{
    public class Location : BaseAuditableEntity
    {
        public string Name { get; set; } = string.Empty; // Baku Airport, City Center Office
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string Country { get; set; } = "Azerbaijan";
        public string? PostalCode { get; set; }

        // Coordinates
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Contact
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }

        // Working Hours
        public string? OpeningHours { get; set; } // JSON format
        public bool Is24Hours { get; set; } = false;

        // Features
        public bool IsAirport { get; set; } = false;
        public bool HasParkingSpace { get; set; } = true;
        public bool IsActive { get; set; } = true;

        // Additional Info
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }

        // Navigation Properties
        public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
        public virtual ICollection<Booking> PickupBookings { get; set; } = new List<Booking>();
        public virtual ICollection<Booking> ReturnBookings { get; set; } = new List<Booking>();
    }

}
