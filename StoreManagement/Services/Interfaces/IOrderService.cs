using StoreManagement.Models.Enums;
using StoreManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StoreManagement.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderListViewModel>> GetAllOrdersAsync();

        Task<OrderFilterViewModel> GetFilteredOrdersAsync(OrderFilterViewModel filter); 

        Task<OrderViewModel?> GetOrderByIdAsync(int id);
        Task<CreateOrderViewModel> GetCreateOrderViewModelAsync(int currentStaffId);
        Task<bool> CreateOrderAsync(CreateOrderViewModel model);
        Task<bool> DeleteOrderAsync(int id);
        Task<EditOrderViewModel?> GetEditOrderViewModelAsync(int orderId);
        Task<bool> UpdateOrderAsync(EditOrderViewModel model);
        Task<bool> DeleteOrderDetailAsync(int orderDetailId);
        Task<bool> UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
        Task<bool> OrderExistsAsync(int id);
        Task<int> GetNextOrderIdAsync();

        // New: get products for a given category (used by AJAX)
        Task<List<SelectListItem>> GetProductsByCategoryAsync(int categoryId);

        Task<DashboardViewModel> GetDashboardDataAsync(string userRole, string userName);
    }
}