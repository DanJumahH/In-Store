using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models.ViewModels
{
    public class OrderViewModel
    {
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Staff Member")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Order date is required")]
        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Shipper")]
        public int? ShipperId { get; set; }

        [Display(Name = "Status")]
        public OrderStatus Status { get; set; }

        public DateTime? StatusUpdatedAt { get; set; }

        // Payment Properties
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        [Display(Name = "Payment Status")]
        public PaymentStatus PaymentStatus { get; set; }

        [Display(Name = "Payment Reference")]
        public string? PaymentReference { get; set; }

        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Payment Notes")]
        [StringLength(500)]
        public string? PaymentNotes { get; set; }

        // For display
        public string? CustomerName { get; set; }
        public string? StaffName { get; set; }
        public string? ShipperName { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }

        // For dropdowns
        public List<SelectListItem>? Customers { get; set; }
        public List<SelectListItem>? Staff { get; set; }
        public List<SelectListItem>? Shippers { get; set; }
        public List<SelectListItem>? StatusOptions { get; set; }

        // Order details (line items)
        public List<OrderDetailViewModel> OrderDetails { get; set; } = new List<OrderDetailViewModel>();

        // Helper method for badge CSS class
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
    }
}

