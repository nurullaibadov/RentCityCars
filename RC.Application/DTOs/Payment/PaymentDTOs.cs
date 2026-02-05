using RC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RC.Application.DTOs.Payment
{
    public class ProcessPaymentRequestDto
    {
        public Guid BookingId { get; set; }
        public PaymentMethod Method { get; set; }
        public decimal Amount { get; set; }

        // Card Details (if applicable)
        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? CVV { get; set; }
    }

    public class PaymentDto
    {
        public Guid Id { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public string? CardLastFourDigits { get; set; }
        public string? InvoiceUrl { get; set; }
    }

    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PaymentDto? Payment { get; set; }
        public string? InvoiceUrl { get; set; }
    }
}
