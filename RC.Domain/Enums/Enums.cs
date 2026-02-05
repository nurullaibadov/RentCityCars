using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Enums
{
    public enum BookingStatus
    {
        Pending = 0,
        Confirmed = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4,
        Rejected = 5
    }

    public enum PaymentStatus
    {
        Pending = 0,
        Completed = 1,
        Failed = 2,
        Refunded = 3,
        PartiallyRefunded = 4
    }

    public enum PaymentMethod
    {
        CreditCard = 0,
        DebitCard = 1,
        Cash = 2,
        BankTransfer = 3,
        Stripe = 4,
        PayPal = 5
    }

    public enum CarStatus
    {
        Available = 0,
        Rented = 1,
        Maintenance = 2,
        OutOfService = 3,
        Reserved = 4
    }

    public enum TransmissionType
    {
        Manual = 0,
        Automatic = 1,
        SemiAutomatic = 2
    }

    public enum FuelType
    {
        Petrol = 0,
        Diesel = 1,
        Electric = 2,
        Hybrid = 3,
        PlugInHybrid = 4
    }

    public enum UserRole
    {
        Customer = 0,
        Driver = 1,
        Admin = 2,
        SuperAdmin = 3
    }

    public enum NotificationType
    {
        Email = 0,
        SMS = 1,
        Push = 2,
        InApp = 3
    }

    public enum MaintenanceType
    {
        Regular = 0,
        Oil = 1,
        Tire = 2,
        Brake = 3,
        Engine = 4,
        Transmission = 5,
        Other = 6
    }

    public enum InsuranceType
    {
        Basic = 0,
        Comprehensive = 1,
        ThirdParty = 2,
        Full = 3
    }

    public enum DriverStatus
    {
        Available = 0,
        Busy = 1,
        OffDuty = 2,
        OnBreak = 3
    }
}
