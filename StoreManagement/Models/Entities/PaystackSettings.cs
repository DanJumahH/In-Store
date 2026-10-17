namespace StoreManagement.Models.Entities
{
    public class PaystackSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string PublicKey { get; set; } = string.Empty;

        public string DefaultCustomerEmail { get; set; } = string.Empty;
    }
}