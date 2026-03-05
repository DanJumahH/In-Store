using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models.ViewModels
{
    public class OrderItemInputViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        // For display
        public string? ProductName { get; set; }
        public decimal? UnitPrice { get; set; }
    }
}
