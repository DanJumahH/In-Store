using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Data;
using StoreManagement.Models.Entities;
using StoreManagement.Models.Enums;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;

namespace StoreManagement.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<OrderFilterViewModel> GetFilteredOrdersAsync(OrderFilterViewModel filter)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .AsQueryable();

            // Apply search term (Order ID or Customer Name)
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.Trim().ToLower();
                query = query.Where(o =>
                    o.OrderId.ToString().Contains(searchTerm) ||
                    (o.Customer != null && o.Customer.CustomerName.ToLower().Contains(searchTerm))
                );
            }

            // Apply status filter
            if (filter.Status.HasValue)
            {
                query = query.Where(o => o.Status == filter.Status.Value);
            }

            // Apply staff filter
            if (filter.StaffId.HasValue)
            {
                query = query.Where(o => o.EmployeeId == filter.StaffId.Value);
            }

            // Apply date range filter
            if (filter.StartDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                // Include the entire end date
                var endDateInclusive = filter.EndDate.Value.AddDays(1);
                query = query.Where(o => o.OrderDate < endDateInclusive);
            }

            // Execute query and map to view model
            var orders = await query
                .Select(o => new OrderListViewModel
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    CustomerName = o.Customer != null ? o.Customer.CustomerName : "N/A",
                    StaffName = o.Staff != null ? o.Staff.FirstName + " " + o.Staff.LastName : "N/A",
                    ShipperName = o.Shipper != null ? o.Shipper.ShipperName : "N/A",
                    ItemCount = o.OrderDetails.Count(),
                    TotalAmount = o.OrderDetails.Sum(od => (od.Product.Price) * (od.Quantity)),
                    Status = o.Status,
                    StatusUpdatedAt = o.StatusUpdatedAt,
                    PaymentStatus = o.PaymentStatus,
                    PaymentMethod = o.PaymentMethod,
                    PaymentDate = o.PaymentDate
                })
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderId)
                .ToListAsync();

            filter.Orders = orders;

            // Populate dropdown options
            filter.StatusOptions = Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .Select(s => new SelectListItem
                {
                    Value = ((int)s).ToString(),
                    Text = s.ToString()
                })
                .ToList();

            filter.StaffOptions = await _context.Staffs
                .AsNoTracking()
                .OrderBy(s => s.LastName)
                .Select(s => new SelectListItem
                {
                    Value = s.StaffId.ToString(),
                    Text = s.FirstName + " " + s.LastName
                })
                .ToListAsync();

            return filter;
        }

        public async Task<IEnumerable<OrderListViewModel>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Select(o => new OrderListViewModel
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    CustomerName = o.Customer != null ? o.Customer.CustomerName : "N/A",
                    StaffName = o.Staff != null ? o.Staff.FirstName + " " + o.Staff.LastName : "N/A",
                    ShipperName = o.Shipper != null ? o.Shipper.ShipperName : "N/A",
                    ItemCount = o.OrderDetails.Count(),
                    TotalAmount = o.OrderDetails.Sum(od => (od.Product.Price) * (od.Quantity)),
                    Status = o.Status,
                    StatusUpdatedAt = o.StatusUpdatedAt
                })
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderId)
                .ToListAsync();
        }

        public async Task<OrderViewModel?> GetOrderByIdAsync(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.Shipper)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.OrderId == id)
                .Select(o => new OrderViewModel
                {
                    OrderId = o.OrderId,
                    CustomerId = o.CustomerId,
                    EmployeeId = o.EmployeeId,
                    OrderDate = DateTime.Today,
                    ShipperId = o.ShipperId,
                    Status = o.Status,
                    StatusUpdatedAt = o.StatusUpdatedAt,
                    CustomerName = o.Customer != null ? o.Customer.CustomerName : "N/A",
                    StaffName = o.Staff != null ? o.Staff.FirstName + " " + o.Staff.LastName : "N/A",
                    ShipperName = o.Shipper != null ? o.Shipper.ShipperName : "N/A",
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailViewModel
                    {
                        OrderDetailId = od.OrderDetailId,
                        OrderId = od.OrderId,
                        ProductId = od.ProductId,
                        Quantity = od.Quantity,
                        ProductName = od.Product != null ? od.Product.ProductName : "N/A",
                        UnitPrice = od.Product != null ? od.Product.Price : 0
                    }).ToList(),
                    PaymentMethod = o.PaymentMethod,
                    PaymentStatus = o.PaymentStatus,
                    PaymentReference = o.PaymentReference,
                    PaymentDate = o.PaymentDate,
                    PaymentNotes = o.PaymentNotes
                })
                .FirstOrDefaultAsync();

            if (order != null)
            {
                order.TotalAmount = order.OrderDetails.Sum(od => od.LineTotal);
                order.TotalItems = order.OrderDetails.Sum(od => od.Quantity);

                // Get status options for dropdown
                order.StatusOptions = Enum.GetValues(typeof(OrderStatus))
                    .Cast<OrderStatus>()
                    .Select(s => new SelectListItem
                    {
                        Value = ((int)s).ToString(),
                        Text = s.ToString()
                    })
                    .ToList();
            }

            return order;
        }

        public async Task<CreateOrderViewModel> GetCreateOrderViewModelAsync(int currentStaffId)
        {
            var customers = await _context.Customers
                .AsNoTracking()
                .OrderBy(c => c.CustomerName)
                .Select(c => new SelectListItem
                {
                    Value = c.CustomerId.ToString(),
                    Text = c.CustomerName
                })
                .ToListAsync();

            var staff = await _context.Staffs
                .AsNoTracking()
                .OrderBy(s => s.LastName)
                .Select(s => new SelectListItem
                {
                    Value = s.StaffId.ToString(),
                    Text = s.FirstName + " " + s.LastName
                })
                .ToListAsync();

            var shippers = await _context.Shippers
                .AsNoTracking()
                .OrderBy(s => s.ShipperName)
                .Select(s => new SelectListItem
                {
                    Value = s.ShipperId.ToString(),
                    Text = s.ShipperName
                })
                .ToListAsync();

            // Load categories so the UI can prompt user to pick one first
            var categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
                .ToListAsync();

            // Important: Do not load ALL products here (to avoid showing them at once).
            // Products will be loaded via AJAX when a category is selected.
            var productSelectList = new List<SelectListItem>();

            return new CreateOrderViewModel
            {
                Customers = customers,
                Staff = staff,
                Shippers = shippers,
                Categories = categories,
                Products = productSelectList,
                OrderDate = DateTime.Today,
                EmployeeId = currentStaffId
            };
        }

        // New: return products for a single category
        public async Task<List<SelectListItem>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _context.Products
                .AsNoTracking()
                .Where(p => p.CategoryId == categoryId)
                .OrderBy(p => p.ProductName)
                .Select(p => new SelectListItem
                {
                    Value = p.ProductId.ToString(),
                    // Include price in text so client-side can parse it (format: "Name - $X.YY")
                    Text = $"{p.ProductName} - ${p.Price:F2}"
                })
                .ToListAsync();

            return products;
        }

        public async Task<bool> CreateOrderAsync(CreateOrderViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // DEBUGGING: Check if we have items
                if (model.Items == null || !model.Items.Any())
                {
                    Console.WriteLine("ERROR: No items in model!");
                    return false;
                }

                Console.WriteLine($"Creating order with {model.Items.Count} items");


                // Get next order ID
                var nextOrderId = await GetNextOrderIdAsync();
                Console.WriteLine($"Next Order ID: {nextOrderId}");

                // Create order
                var order = new Order
                {
                    OrderId = nextOrderId,
                    CustomerId = model.CustomerId,
                    EmployeeId = model.EmployeeId,
                    OrderDate = model.OrderDate,
                    ShipperId = model.ShipperId,
                    Status = OrderStatus.Pending,
                    StatusUpdatedAt = DateTime.Now
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();
                Console.WriteLine($"Order {nextOrderId} created successfully");

                // Create order details
                if (model.Items != null && model.Items.Any())
                {
                    foreach (var item in model.Items.Where(i => i.ProductId > 0 && i.Quantity > 0))
                    {
                        Console.WriteLine($"Adding item: ProductId={item.ProductId}, Quantity={item.Quantity}");

                        var orderDetail = new OrderDetail
                        {
                            OrderId = nextOrderId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            CreatedAt = DateTime.Now
                        };

                        _context.OrderDetails.Add(orderDetail);
                    }


                    var detailsSaved = await _context.SaveChangesAsync();
                    Console.WriteLine($"OrderDetails saved: {detailsSaved} records");
                }

                await transaction.CommitAsync();
                Console.WriteLine("Transaction Committed Successfully");
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"ERROR in CreateOrderAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    return false;
                }

                order.Status = newStatus;
                order.StatusUpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                Console.WriteLine($"Order {orderId} status updated to: {newStatus}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR updating order status: {ex.Message}");
                return false;
            }
        }

        public async Task<EditOrderViewModel?> GetEditOrderViewModelAsync(int orderId)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return null;

            var viewModel = new EditOrderViewModel
            {
                OrderId = order.OrderId,
                CustomerId = order.CustomerId,
                EmployeeId = order.EmployeeId,
                OrderDate = order.OrderDate,
                ShipperId = order.ShipperId,
                Status = order.Status,
                ExistingItems = order.OrderDetails.Select(od => new OrderDetailViewModel
                {
                    OrderDetailId = od.OrderDetailId,
                    OrderId = od.OrderId,
                    ProductId = od.ProductId,
                    Quantity = od.Quantity,
                    ProductName = od.Product?.ProductName ?? "N/A",
                    UnitPrice = od.Product?.Price ?? 0
                }).ToList()
            };

            // Load dropdowns
            viewModel.Customers = await _context.Customers
                .AsNoTracking()
                .OrderBy(c => c.CustomerName)
                .Select(c => new SelectListItem
                {
                    Value = c.CustomerId.ToString(),
                    Text = c.CustomerName
                })
                .ToListAsync();

            viewModel.Staff = await _context.Staffs
                .AsNoTracking()
                .OrderBy(s => s.LastName)
                .Select(s => new SelectListItem
                {
                    Value = s.StaffId.ToString(),
                    Text = s.FirstName + " " + s.LastName
                })
                .ToListAsync();

            viewModel.Shippers = await _context.Shippers
                .AsNoTracking()
                .OrderBy(s => s.ShipperName)
                .Select(s => new SelectListItem
                {
                    Value = s.ShipperId.ToString(),
                    Text = s.ShipperName
                })
                .ToListAsync();

            var products = await _context.Products
                .AsNoTracking()
                .OrderBy(p => p.ProductName)
                .Select(p => new
                {
                    ProductId = p.ProductId.ToString(),
                    ProductName = p.ProductName,
                    Price = p.Price
                })
                .ToListAsync();

            viewModel.Products = products.Select(p => new SelectListItem
            {
                Value = p.ProductId.ToString(),
                Text = $"{p.ProductName} - ${p.Price:F2}"
            }).ToList();

            return viewModel;
        }

        public async Task<bool> UpdateOrderAsync(EditOrderViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Get the existing order
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == model.OrderId);

                if (order == null)
                {
                    Console.WriteLine($"Order {model.OrderId} not found");
                    return false;
                }

                // Update order header
                order.CustomerId = model.CustomerId;
                order.EmployeeId = model.EmployeeId;
                order.OrderDate = model.OrderDate;
                order.ShipperId = model.ShipperId;
                // Keep the existing status - it can be updated separately via UpdateOrderStatusAsync

                await _context.SaveChangesAsync();
                Console.WriteLine($"Order {model.OrderId} header updated successfully");

                // Add new items if any
                if (model.NewItems != null && model.NewItems.Any())
                {
                    foreach (var item in model.NewItems.Where(i => i.ProductId > 0 && i.Quantity > 0))
                    {
                        Console.WriteLine($"Adding new item: ProductId={item.ProductId}, Quantity={item.Quantity}");

                        var orderDetail = new OrderDetail
                        {
                            OrderId = model.OrderId,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            CreatedAt = DateTime.Now
                        };

                        _context.OrderDetails.Add(orderDetail);
                    }

                    await _context.SaveChangesAsync();
                    Console.WriteLine($"New items added successfully");
                }

                await transaction.CommitAsync();
                Console.WriteLine("Transaction committed successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in UpdateOrderAsync: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> DeleteOrderDetailAsync(int orderDetailId)
        {
            try
            {
                var orderDetail = await _context.OrderDetails.FindAsync(orderDetailId);
                if (orderDetail == null)
                {
                    Console.WriteLine($"OrderDetail {orderDetailId} not found");
                    return false;
                }

                _context.OrderDetails.Remove(orderDetail);
                await _context.SaveChangesAsync();

                Console.WriteLine($"OrderDetail {orderDetailId} deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in DeleteOrderDetailAsync: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Delete order details first
                var orderDetails = await _context.OrderDetails
                    .Where(od => od.OrderId == id)
                    .ToListAsync();

                _context.OrderDetails.RemoveRange(orderDetails);

                // Delete order
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                    return false;

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> OrderExistsAsync(int id)
        {
            return await _context.Orders.AnyAsync(o => o.OrderId == id);
        }

        public async Task<int> GetNextOrderIdAsync()
        {
            // Since OrderId is not auto-generated, get the max and add 1
            var maxOrderId = await _context.Orders
                .AsNoTracking()
                .MaxAsync(o => (int?)o.OrderId) ?? 10000;

            return maxOrderId + 1;
        }


        public async Task<DashboardViewModel> GetDashboardDataAsync(string userRole, string userName)
        {
            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var dashboard = new DashboardViewModel
            {
                UserRole = userRole,
                UserName = userName
            };

            // === ADMIN: Load Everything ===
            if (userRole == "Admin")
            {
                await LoadSalesMetrics(dashboard, today, weekStart, monthStart);
                await LoadInventoryMetrics(dashboard);
                await LoadCustomerMetrics(dashboard, monthStart);
                await LoadStaffMetrics(dashboard);
            }
            // === INVENTORY MANAGER: Products, Categories, Suppliers, Shippers ===
            else if (userRole == "InventoryManager")
            {
                await LoadInventoryMetrics(dashboard);
            }
            // === SALES STAFF: Orders and Customers ===
            else if (userRole == "SalesStaff")
            {
                await LoadSalesMetrics(dashboard, today, weekStart, monthStart);
                await LoadCustomerMetrics(dashboard, monthStart);
            }

            return dashboard;
        }

        private async Task LoadSalesMetrics(DashboardViewModel dashboard, DateTime today, DateTime weekStart, DateTime monthStart)
        {
            var tomorrow = today.AddDays(1);

            // Optimize: Get counts and sums directly from database instead of loading all data
            // Today's Stats
            dashboard.TodayOrdersCount = await _context.Orders
                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow)
                .CountAsync();

            dashboard.TodayRevenue = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Where(od => od.Order.OrderDate >= today && od.Order.OrderDate < tomorrow)
                .SumAsync(od => od.Product.Price * od.Quantity);

            // Week Stats
            dashboard.WeekOrdersCount = await _context.Orders
                .Where(o => o.OrderDate >= weekStart)
                .CountAsync();

            dashboard.WeekRevenue = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Where(od => od.Order.OrderDate >= weekStart)
                .SumAsync(od => od.Product.Price * od.Quantity);

            // Month Stats
            dashboard.MonthOrdersCount = await _context.Orders
                .Where(o => o.OrderDate >= monthStart)
                .CountAsync();

            dashboard.MonthRevenue = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Where(od => od.Order.OrderDate >= monthStart)
                .SumAsync(od => od.Product.Price * od.Quantity);

            // Overall Stats
            dashboard.TotalOrders = await _context.Orders.CountAsync();

            dashboard.TotalRevenue = await _context.OrderDetails
                .Include(od => od.Product)
                .SumAsync(od => od.Product.Price * od.Quantity);

            // Orders by Status
            var statusGroups = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            dashboard.OrdersByStatus = statusGroups.ToDictionary(g => g.Status, g => g.Count);

            foreach (OrderStatus status in Enum.GetValues(typeof(OrderStatus)))
            {
                if (!dashboard.OrdersByStatus.ContainsKey(status))
                    dashboard.OrdersByStatus[status] = 0;
            }

            // Recent Orders - Only load top 10 with minimal data
            dashboard.RecentOrders = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.Shipper)
                .OrderByDescending(o => o.OrderDate)
                .ThenByDescending(o => o.OrderId)
                .Take(10)
                .Select(o => new OrderListViewModel
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    CustomerName = o.Customer.CustomerName,
                    StaffName = o.Staff.FirstName + " " + o.Staff.LastName,
                    ShipperName = o.Shipper != null ? o.Shipper.ShipperName : "N/A",
                    ItemCount = o.OrderDetails.Count(),
                    TotalAmount = o.OrderDetails.Sum(od => od.Product.Price * od.Quantity),
                    Status = o.Status,
                    StatusUpdatedAt = o.StatusUpdatedAt
                })
                .ToListAsync();

            // Top Customers - Optimized to only load needed data
            var topCustomerIds = await _context.Orders
                .GroupBy(o => o.CustomerId)
                .Select(g => new { CustomerId = g.Key, OrderCount = g.Count() })
                .OrderByDescending(x => x.OrderCount)
                .Take(5)
                .Select(x => x.CustomerId)
                .ToListAsync();

            if (topCustomerIds.Any())
            {
                var customerData = await _context.Orders
                    .AsNoTracking()
                    .Where(o => topCustomerIds.Contains(o.CustomerId))
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .ToListAsync();

                dashboard.TopCustomers = customerData
                    .GroupBy(o => new { o.CustomerId, o.Customer.CustomerName })
                    .Select(g => new TopCustomerViewModel
                    {
                        CustomerId = g.Key.CustomerId,
                        CustomerName = g.Key.CustomerName,
                        OrderCount = g.Count(),
                        TotalSpent = g.Sum(o => o.OrderDetails.Sum(od => od.Product.Price * od.Quantity))
                    })
                    .OrderByDescending(c => c.TotalSpent)
                    .Take(5)
                    .ToList();
            }
            else
            {
                dashboard.TopCustomers = new List<TopCustomerViewModel>();
            }
        }


        private async Task LoadInventoryMetrics(DashboardViewModel dashboard)
        {
            // Product Stats
            dashboard.TotalProducts = await _context.Products.CountAsync();
            dashboard.TotalCategories = await _context.Categories.CountAsync();
            dashboard.TotalSuppliers = await _context.Suppliers.CountAsync();
            dashboard.TotalShippers = await _context.Shippers.CountAsync();
            dashboard.LowStockProducts = await _context.Products
                .Where(p => p.Stock < 10)
                .CountAsync();

            // Recent Products (last 10 added)
            dashboard.RecentProducts = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .OrderByDescending(p => p.ProductId)
                .Take(10)
                .Select(p => new RecentProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    CategoryName = p.Category != null ? p.Category.CategoryName : "N/A",
                    Price = p.Price,
                    UnitsInStock = p.Stock
                })
                .ToListAsync();

            // Top Products by Sales
            dashboard.TopProducts = await _context.OrderDetails
                .AsNoTracking()
                .Include(od => od.Product)
                .GroupBy(od => new { od.ProductId, od.Product.ProductName, od.Product.Stock })
                .Select(g => new TopProductViewModel
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    TotalQuantity = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.Product.Price * od.Quantity),
                    UnitsInStock = g.Key.Stock
                })
                .OrderByDescending(p => p.TotalQuantity)
                .Take(5)
                .ToListAsync();

            // Category Stats
            dashboard.CategoryStats = await _context.Categories
                .AsNoTracking()
                .Include(c => c.Products)
                .Select(c => new CategoryStatsViewModel
                {
                    CategoryName = c.CategoryName,
                    ProductCount = c.Products.Count,
                    TotalValue = c.Products.Sum(p => p.Price * p.Stock)
                })
                .OrderByDescending(c => c.ProductCount)
                .Take(5)
                .ToListAsync();
        }

        private async Task LoadCustomerMetrics(DashboardViewModel dashboard, DateTime monthStart)
        {
            dashboard.TotalCustomers = await _context.Customers.CountAsync();

            // Assuming Customers table has a CreatedAt or similar field
            // If not, you can remove this or adjust based on your schema
            // dashboard.NewCustomersThisMonth = await _context.Customers
            //     .Where(c => c.CreatedAt >= monthStart)
            //     .CountAsync();
        }

        private async Task LoadStaffMetrics(DashboardViewModel dashboard)
        {
            dashboard.TotalStaff = await _context.Staffs.CountAsync();

            // Top Staff by Orders Processed - Only load data for top 5 staff
            var topStaffIds = await _context.Orders
                .GroupBy(o => o.EmployeeId)
                .Select(g => new { StaffId = g.Key, OrderCount = g.Count() })
                .OrderByDescending(x => x.OrderCount)
                .Take(5)
                .Select(x => x.StaffId)
                .ToListAsync();

            if (topStaffIds.Any())
            {
                var staffOrders = await _context.Orders
                    .AsNoTracking()
                    .Where(o => topStaffIds.Contains(o.EmployeeId))
                    .Include(o => o.Staff)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .ToListAsync();

                dashboard.TopStaff = staffOrders
                    .GroupBy(o => new { o.EmployeeId, o.Staff.FirstName, o.Staff.LastName })
                    .Select(g => new StaffPerformanceViewModel
                    {
                        StaffId = g.Key.EmployeeId,
                        StaffName = $"{g.Key.FirstName} {g.Key.LastName}",
                        OrdersProcessed = g.Count(),
                        RevenueGenerated = g.Sum(o => o.OrderDetails.Sum(od => od.Product.Price * od.Quantity))
                    })
                    .OrderByDescending(s => s.OrdersProcessed)
                    .Take(5)
                    .ToList();
            }
            else
            {
                dashboard.TopStaff = new List<StaffPerformanceViewModel>();
            }
        }



    }
}