using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models.Entities;

public partial class Supplier
{
    [Key]
    public int SupplierId { get; set; }

    [Required]
    [StringLength(100)]
    public string? SupplierName { get; set; }

    [StringLength(100)]
    public string? ContactName { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(50)]
    public string? City { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [StringLength(50)]
    public string? Country { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }


    public ICollection<Product> Products { get; set; } = new List<Product>();
}
