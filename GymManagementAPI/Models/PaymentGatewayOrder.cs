namespace GymManagementAPI.Models
{
    public class PaymentGatewayOrder
    {
        public int Id { get; set; }

        // link back to Payment (optional one-to-one)
        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }

        // Gateway-specific
        public string OrderId { get; set; } = null!;           // unique cf_order_id
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string Status { get; set; } = "CREATED";        // CREATED / EXPIRED / COMPLETED ...
        public string? RequestJson { get; set; }               // raw request for debugging
        public string? ResponseJson { get; set; }              // raw response stored
        public string? PaymentSessionId { get; set; }          // e.g. "session_..."

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
