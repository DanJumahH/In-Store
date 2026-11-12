namespace StoreManagement.Models.ViewModels
{
    public class PaymentInitiateRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
    }
}