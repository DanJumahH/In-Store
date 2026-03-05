using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StoreManagement.Models.ViewModels
{
    public class CreateOrderViewModel
    {
        [Required(ErrorMessage = "Customer is required")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Staff Member")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Order date is required")]
        [Display(Name = "Order Date")]
        [DataType(DataType.Date)]
        public DateTime OrderDate { get; set; } = DateTime.Today;

        [Display(Name = "Shipper")]
        public int? ShipperId { get; set; }

        // For dropdowns
        public List<SelectListItem>? Customers { get; set; }
        public List<SelectListItem>? Staff { get; set; }
        public List<SelectListItem>? Shippers { get; set; }
        public List<SelectListItem>? Products { get; set; }
        public List<SelectListItem>? Categories { get; set; } // New: Categories dropdown

        // Line items to add
        public List<OrderItemInputViewModel> Items { get; set; } = new List<OrderItemInputViewModel>();
    }
}
