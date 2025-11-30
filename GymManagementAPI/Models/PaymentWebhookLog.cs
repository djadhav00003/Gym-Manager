namespace GymManagementAPI.Models
{
    public class PaymentWebhookLog
    {
        public int Id { get; set; }

        // optional link to Payment (if we could resolve by OrderId)
        public int? PaymentId { get; set; }
        public Payment? Payment { get; set; }

        public string OrderId { get; set; } = null!;   // order id from webhook payload
        public string EventType { get; set; } = null!; // e.g. "PAYMENT_CAPTURED"
        public string Payload { get; set; } = null!;   // raw webhook JSON
        public bool Processed { get; set; } = false;
        public string? ProcessingResult { get; set; }  // notes or error
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        // NEW - key used for deduplication (unique index in DB)
       public string? WebhookKey { get; set; }
    }
}
