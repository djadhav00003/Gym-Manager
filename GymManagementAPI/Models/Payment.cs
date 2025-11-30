namespace GymManagementAPI.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        public User? User { get; set; }

        public int? PlanId { get; set; }
        public Plan? Plan { get; set; }

        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }

        // gateway fields
        public string? OrderId { get; set; }         // cashfree cf_order_id
        public string? TransactionId { get; set; }   // cf transaction id
        public string PaymentStatus { get; set; } = "PENDING"; // PENDING | SUCCESS | FAILED | REFUNDED
        public string? PaymentGateway { get; set; }  // e.g. "Cashfree"
        public string Currency { get; set; } = "INR";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // 1:1 navigation to gateway order record (optional)
        public PaymentGatewayOrder? PaymentGatewayOrder { get; set; }

        // webhook logs (1:many)
        public ICollection<PaymentWebhookLog> WebhookLogs { get; set; } = new List<PaymentWebhookLog>();
    }

}
