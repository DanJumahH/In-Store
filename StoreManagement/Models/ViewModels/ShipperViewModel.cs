using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models.ViewModels
{
    public class ShipperViewModel
    {
        public int ShipperId { get; set; }

        [Required(ErrorMessage = "Shipper name is required")]
        [StringLength(255, ErrorMessage = "Shipper name cannot exceed 255 characters")]
        [Display(Name = "Shipper Name")]
        public string ShipperName { get; set; } = string.Empty;

        [StringLength(15, ErrorMessage = "Phone cannot exceed 15 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        // Display property for showing order count
        [Display(Name = "Number of Orders")]
        public int OrderCount { get; set; }
    }
}