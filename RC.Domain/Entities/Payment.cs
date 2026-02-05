using RC.Domain.Common;
using RC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Domain.Entities
{
    public class Payment : BaseAuditableEntity
    {
        // User & Booking
        public string UserId { get; set; } = string.Empty;
        public virtual AppUser User { get; set; } = null!;

        public Guid BookingId { get; set; }
        public virtual Booking Booking { get; set; } = null!;

        // Payment Details
        public string TransactionId { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;

        public decimal Amount { get; set; }
        public decimal RefundedAmount { get; set; } = 0;

        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        // Card Information (if applicable - store securely)
        public string? CardLastFourDigits { get; set; }
        public string? CardBrand { get; set; } // Visa, Mastercard, etc.

        // Timestamps
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }

        // Additional Info
        public string? PaymentGatewayResponse { get; set; }
        public string? FailureReason { get; set; }
        public string? Notes { get; set; }

        // Invoice
        public string? InvoiceUrl { get; set; }
        public bool InvoiceGenerated { get; set; } = false;
    }

}
