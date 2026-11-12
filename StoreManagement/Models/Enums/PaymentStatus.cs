namespace StoreManagement.Models.Enums
{
    public enum PaymentStatus
    {
        Unpaid = 0,
        Paid = 1,
        Failed = 2,
        Pending = 3     // Payment initiated but not completed
    }
}