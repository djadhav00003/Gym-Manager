namespace GymManagementAPI.Models
{
    public class CashfreeOptions
    {
        public string AppId { get; set; } = "";
        public string Secret { get; set; } = "";
        public string BaseUrl { get; set; } = "";     // e.g. https://sandbox.cashfree.com
        public string CreateOrderPath { get; set; } = "/api/v1/create-order"; // set to actual path from docs
        public string VerifyPaymentPath { get; set; } = "/api/v1/verify";     // optional
        public string WebhookSecret { get; set; } = ""; // optional: if provided by Cashfree
    }
}
