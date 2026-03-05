using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models.ViewModels
{
    public class CustomerViewModel
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Customer name is required")]
        [StringLength(255, ErrorMessage = "Customer name cannot exceed 255 characters")]
        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Contact name cannot exceed 255 characters")]
        [Display(Name = "Contact Name")]
        public string? ContactName { get; set; }

        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters")]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [StringLength(255, ErrorMessage = "City cannot exceed 255 characters")]
        [Display(Name = "City")]
        public string? City { get; set; }

        [StringLength(20, ErrorMessage = "Postal code cannot exceed 20 characters")]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [StringLength(255, ErrorMessage = "Country cannot exceed 255 characters")]
        [Display(Name = "Country")]
        public string? Country { get; set; }

        [Display(Name = "Created Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = false)]
        public DateTime? Created { get; set; }

        // Display property for showing order count
        [Display(Name = "Number of Orders")]
        public int OrderCount { get; set; }
    }
}
