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


        // Order details (line items)
        public List<OrderDetailViewModel> OrderDetails { get; set; } = new List<OrderDetailViewModel>();
        }

}

