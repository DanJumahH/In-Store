using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models.Entities;

public partial class Staff
{
    [Key]
    public int StaffId { get; set; }

    [Required]
    [StringLength(50)]
    public string? LastName { get; set; }

    [Required]
    [StringLength(50)]
    public string? FirstName { get; set; }

    public DateOnly? BirthDate { get; set; }

    [Required]
    [StringLength(100)]
    public string? Email { get; set; }

    [StringLength(50)]
    public string? StaffRole { get; set; }

    public string? Notes { get; set; }

    public ICollection<Order> Orders { get; set; }
}
