using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagement.Models.Enums;

namespace StoreManagement.Models.ViewModels
{
    public class EditOrderViewModel
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

        // For dropdowns
        public List<SelectListItem>? Customers { get; set; }
        public List<SelectListItem>? Staff { get; set; }
        public List<SelectListItem>? Shippers { get; set; }
        public List<SelectListItem>? Products { get; set; }

        // Existing order items
        public List<OrderDetailViewModel> ExistingItems { get; set; } = new List<OrderDetailViewModel>();

        // New items to add
        public List<OrderItemInputViewModel> NewItems { get; set; } = new List<OrderItemInputViewModel>();
    }
}