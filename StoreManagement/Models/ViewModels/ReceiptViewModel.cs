using StoreManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models.ViewModels
{
    public class ReceiptViewModel
    {
        public int OrderId { get; set; }

        public string? StoreName { get; set; }

        // Customer Info
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }

        // Staff Info
        public string? StaffName { get; set; }

        // Order Info
        public DateTime OrderDate { get; set; }
        public DateTime? PaymentDate { get; set; }

        public string? PaymentReference { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        // Line Items
        public List<ReceiptItemViewModel> Items { get; set; } = new();

        public decimal Subtotal { get; set; }
        public decimal Charges { get; set; }
        public decimal Total { get; set; }

        // Display Properties
        public string OrderDateFormatted => OrderDate.ToString("dd/MM/yyyy HH:mm");
        public string PaymentDateFormatted => PaymentDate?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
    }

    public class ReceiptItemViewModel
    {
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total => Quantity * UnitPrice;
    }
}