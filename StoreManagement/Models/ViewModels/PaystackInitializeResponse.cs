namespace StoreManagement.Models.ViewModels
{
    public class PaystackInitializeResponse
    {
        public bool Status { get; set; }
        public string Message { get; set; } = string.Empty;
        public string AuthorizationUrl { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public string AccessCode { get; set; } = string.Empty;
    }
}