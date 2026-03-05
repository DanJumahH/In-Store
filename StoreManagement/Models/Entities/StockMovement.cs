using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreManagement.Models.Entities;

public class StockMovement
{
    [Key]
    public int StockMovementId { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public int Change { get; set; } // positive = increase, negative = decrease

    [StringLength(200)]
    public string? Reason { get; set; } // e.g. "Order #123", "Manual Adjustment", "Purchase Receipt"

    public string? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}