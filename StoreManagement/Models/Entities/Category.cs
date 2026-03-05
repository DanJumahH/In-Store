using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreManagement.Models.Entities;

public partial class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(255)]
    public string? CategoryName { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }


    public ICollection<Product> Products { get; set; } = new List<Product>();
}
