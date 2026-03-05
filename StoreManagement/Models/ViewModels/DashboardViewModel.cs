using StoreManagement.Models.Enums;

namespace StoreManagement.Models.ViewModels
{
    public class DashboardViewModel
    {
        // User Role Info
        public string UserRole { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        // === SALES METRICS (Admin & SalesStaff) ===
        public int TodayOrdersCount { get; set; }
        public decimal TodayRevenue { get; set; }
        public int WeekOrdersCount { get; set; }
        public decimal WeekRevenue { get; set; }
        public int MonthOrdersCount { get; set; }
        public decimal MonthRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public Dictionary<OrderStatus, int> OrdersByStatus { get; set; } = new Dictionary<OrderStatus, int>();
        public List<OrderListViewModel> RecentOrders { get; set; } = new List<OrderListViewModel>();
        public List<TopCustomerViewModel> TopCustomers { get; set; } = new List<TopCustomerViewModel>();

        // === INVENTORY METRICS (Admin & InventoryManager) ===
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalShippers { get; set; }
        public int LowStockProducts { get; set; }
        public List<RecentProductViewModel> RecentProducts { get; set; } = new List<RecentProductViewModel>();
        public List<TopProductViewModel> TopProducts { get; set; } = new List<TopProductViewModel>();
        public List<CategoryStatsViewModel> CategoryStats { get; set; } = new List<CategoryStatsViewModel>();

        // === CUSTOMER METRICS (Admin & SalesStaff) ===
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }

        // === STAFF METRICS (Admin only) ===
        public int TotalStaff { get; set; }
        public List<StaffPerformanceViewModel> TopStaff { get; set; } = new List<StaffPerformanceViewModel>();
    }

    public class TopProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public int UnitsInStock { get; set; }
    }

    public class TopCustomerViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

    public class RecentProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int UnitsInStock { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CategoryStatsViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int ProductCount { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class StaffPerformanceViewModel
    {
        public int StaffId { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public int OrdersProcessed { get; set; }
        public decimal RevenueGenerated { get; set; }
    }
}