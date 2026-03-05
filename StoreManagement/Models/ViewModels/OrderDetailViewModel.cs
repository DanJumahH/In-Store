using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models.ViewModels
{
    public class OrderDetailViewModel
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        // For display
        public string? ProductName { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal LineTotal => (UnitPrice ?? 0) * Quantity;
    }
}
