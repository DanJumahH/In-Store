using System;
using System.Collections.Generic;

namespace StoreManagement.Models.Entities;

public partial class Shipper
{
    public int ShipperId { get; set; }

    public string? ShipperName { get; set; }

    public string? Phone { get; set; }


    public ICollection<Order> Orders { get; set; }
}
