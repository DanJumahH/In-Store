using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models.ViewModels
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Category name is required")]
        [StringLength(255, ErrorMessage = "Category name cannot exceed 255 characters")]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Description cannot exceed 255 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // Display property for showing product count
        [Display(Name = "Number of Products")]
        public int ProductCount { get; set; }
    }
}
