namespace StoreManagement.Models.ViewModels
{
    public class QuickCustomerCreateViewModel
    {
        public string CustomerName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}
