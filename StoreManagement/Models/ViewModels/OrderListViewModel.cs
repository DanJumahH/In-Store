using StoreManagement.Models.Enums;

namespace StoreManagement.Models.ViewModels
{
    public class OrderListViewModel
    {
        public int OrderId { get; set; }
        public DateTime? OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? StaffName { get; set; }
        public string? ShipperName { get; set; }
        public int ItemCount { get; set; }
        public decimal TotalAmount { get; set; }

        public OrderStatus Status { get; set; }
        public DateTime? StatusUpdatedAt { get; set; }
        
        // Payment Info
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public DateTime? PaymentDate { get; set; }

        // Helper methods for badge CSS class
        public string GetStatusBadgeClass()
        {
            return Status switch
            {
                OrderStatus.Pending => "bg-warning text-dark",
                OrderStatus.Paid => "bg-info text-white",
                OrderStatus.Delivered => "bg-success text-white",
                _ => "bg-secondary text-white"
            };
        }

        public string GetPaymentStatusBadgeClass()
        {
            return PaymentStatus switch
            {
                PaymentStatus.Unpaid => "bg-danger text-white",
                PaymentStatus.Paid => "bg-success text-white",
                PaymentStatus.Pending => "bg-warning text-dark",
                PaymentStatus.Failed => "bg-secondary text-white",
                _ => "bg-secondary text-white"
            };
        }

        public string GetPaymentMethodBadge()
        {
            return PaymentMethod switch
            {
                PaymentMethod.Manual => "badge bg-primary",
                PaymentMethod.Paystack => "badge bg-success",
                _ => "badge bg-secondary"
            };
        }
    }
}
