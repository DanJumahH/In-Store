namespace StoreManagement.Models.ViewModels
{
    public class PaystackVerifyResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public int Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
    }
}