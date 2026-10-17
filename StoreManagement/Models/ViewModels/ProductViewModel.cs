using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagement.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(255, ErrorMessage = "Product Name cannot exceed 255 characters")]
        [Display(Name = "Product Name")]
        public string? ProductName { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Supplier")]
        public int? SupplierId { get; set; }

        [StringLength(255, ErrorMessage = "Unit cannot exceed 255 characters")]
        [Display(Name = "Unit")]
        public string? Unit { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive value")]
        [Display(Name = "Price (NGN)")]
        [DisplayFormat(DataFormatString = "{0:C2}", ApplyFormatInEditMode = false)] // Currency format
        public decimal Price { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "NGN";

        // Stock management UI
        [Range(0, int.MaxValue, ErrorMessage = "Stock must be zero or greater")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [StringLength(50, ErrorMessage = "Stock unit cannot exceed 50 characters")]
        [Display(Name = "Stock Unit")]
        public string? StockUnit { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Reorder level must be zero or greater")]
        [Display(Name = "Reorder Level")]
        public int ReorderLevel { get; set; }

        [Display(Name = "Unit Stock")]
        public string UnitStock => $"{Stock} {StockUnit}";

        public string? CategoryName { get; set; }
        public string? SupplierName { get; set; }

        // Dropdown lists for Categories and Suppliers
        public List<SelectListItem>? Categories { get; set; }
        public List<SelectListItem>? Suppliers { get; set; }
    }

    public class ProductListViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string UnitStock => $"{Stock} {StockUnit}";
        public decimal? Price { get; set; }
        public string? CategoryName { get; set; }
        public string? SupplierName { get; set; }

        public int Stock { get; set; }
        public string? StockUnit { get; set; }
        public int ReorderLevel { get; set; }
    }   
}
