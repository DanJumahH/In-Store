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

        // Helper method for badge CSS class
        public string GetStatusBadgeClass()
        {
            return Status switch
            {
                OrderStatus.Pending => "bg-warning text-dark",
                //OrderStatus.Processing => "bg-info text-white",
                //OrderStatus.Shipped => "bg-primary text-white",
                OrderStatus.Delivered => "bg-success text-white",
                //OrderStatus.Cancelled => "bg-danger text-white",
                _ => "bg-secondary text-white"
            };
        }
    }
}
