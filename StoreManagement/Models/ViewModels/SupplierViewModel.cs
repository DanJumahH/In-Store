using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models.ViewModels
{
    public class SupplierViewModel
    {
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(100, ErrorMessage = "Supplier name cannot exceed 100 characters")]
        [Display(Name = "Supplier Name")]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Contact name cannot exceed 100 characters")]
        [Display(Name = "Contact Name")]
        public string? ContactName { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [StringLength(20, ErrorMessage = "City cannot exceed 20 characters")]
        [Display(Name = "City")]
        public string? City { get; set; }

        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [StringLength(20, ErrorMessage = "Country cannot exceed 20 characters")]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [StringLength(20, ErrorMessage = "Phone cannot exceed 20 characters")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Phone")]
        public string? Phone { get; set; }

        // Display property for showing product count
        [Display(Name = "Number of Products")]
        public int ProductCount { get; set; }
    }
}
