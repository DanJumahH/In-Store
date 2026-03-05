using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;

namespace StoreManagement.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IOrderService orderService, ILogger<DashboardController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            try
            {
                // Detect user role
                var userRole = User.IsInRole("Admin") ? "Admin"
                             : User.IsInRole("InventoryManager") ? "InventoryManager"
                             : User.IsInRole("SalesStaff") ? "SalesStaff"
                             : "Guest";

                var userName = User.Identity?.Name ?? "User";

                var dashboardData = await _orderService.GetDashboardDataAsync(userRole, userName);

                // If data is null, return an empty model
                if (dashboardData == null)
                {
                    _logger.LogWarning("Dashboard data returned null");
                    dashboardData = new DashboardViewModel
                    {
                        UserRole = userRole,
                        UserName = userName
                    };
                    TempData["Error"] = "Unable to load dashboard data. Please try again.";
                }

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["Error"] = "An error occurred while loading the dashboard.";

                // Return a basic model instead of null
                var userRole = User.IsInRole("Admin") ? "Admin"
                             : User.IsInRole("InventoryManager") ? "InventoryManager"
                             : User.IsInRole("SalesStaff") ? "SalesStaff"
                             : "Guest";

                return View(new DashboardViewModel
                {
                    UserRole = userRole,
                    UserName = User.Identity?.Name ?? "User"
                });
            }
        }
    }
}