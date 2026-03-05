using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreManagement.Models.Entities;

public partial class Product
{
    [Key]
    public int ProductId { get; set; }

    [Required]
    [StringLength(255)]
    public string ProductName { get; set; }

    public int? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    [StringLength(255)]
    public string? Unit { get; set; }

    [Column(TypeName = "money")]
    public decimal Price { get; set; }

    // stock management
    // current available stock for the product
    public int Stock { get; set; } = 0;               // current available quantity

    [StringLength(50)]
    public string? StockUnit { get; set; }            // e.g. "cartons", "packs", "pcs"
    public int ReorderLevel { get; set; } = 0;        // alert threshold


    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
