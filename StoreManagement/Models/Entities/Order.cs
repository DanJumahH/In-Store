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

    public ICollection<OrderDetail> OrderDetails { get; set; } 
}
