using System.Collections.Generic;

namespace StoreManagement.Services.DTOs
{
    public class OrderLineRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderRequest
    {
        public int CustomerId { get; set; }
        public int? StaffId { get; set; }
        public int? ShipperId { get; set; }

        public List<OrderLineRequest> Lines { get; set; } = new();
    }
}
