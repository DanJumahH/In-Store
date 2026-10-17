using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagement.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StoreManagement.Models.Entities;

public partial class Order
{
    [Key]
    public int OrderId { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } 
    
    public int EmployeeId { get; set; }
    public Staff Staff { get; set; }

    public DateTime OrderDate { get; set; }

    public int? ShipperId { get; set; }
    public Shipper? Shipper { get; set; }

    // Order Status
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime? StatusUpdatedAt { get; set; }

    // Payment
    [MaxLength(100)]
    public string? PaymentReference { get; set; }

    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Manual;

    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

    public DateTime? PaymentDate { get; set; }

    [MaxLength(500)]
    public string? PaymentNotes { get; set; }

    public ICollection<OrderDetail> OrderDetails { get; set; } 
}
