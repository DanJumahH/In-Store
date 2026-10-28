using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models.Entities;

public partial class Customer
{
    [Key]
    public int CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public string? ContactName { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? PostalCode { get; set; }

    public string? Country { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address")]
    [MaxLength(256)]
    public string? Email { get; set; }

    public DateTime? Created { get; set; }

    public ICollection<Order> Orders { get; set; }
}
