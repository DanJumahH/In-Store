using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StoreManagement.Models;
using StoreManagement.Models.Enums;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StoreManagement.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;
        private readonly UserManager<AppUser> _userManager;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger, UserManager<AppUser> userManager)
        {
            _orderService = orderService;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: Order
        public async Task<IActionResult> Index(OrderFilterViewModel filter)
        {
            try
            {
                var result = await _orderService.GetFilteredOrdersAsync(filter);
                return View(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders");
                TempData["Error"] = "An error occurred while loading orders.";
                return View(new OrderFilterViewModel());
            }

            //try
            //{
            //    var orders = await _orderService.GetAllOrdersAsync();
            //    return View(orders);
            //}
            //catch (Exception ex)
            //{
            //    _logger.LogError(ex, "Error retrieving orders");
            //    TempData["Error"] = "An error occurred while loading orders.";
            //    return View(new List<OrderListViewModel>());
            //}
        }

        // GET: Order/Detail s/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var order = await _orderService.GetOrderByIdAsync(id.Value);
                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order details for ID: {OrderId}", id);
                TempData["Error"] = "An error occurred while loading order details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Order/Create
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> Create()
        {
            try
            {
                // Get current user's staff ID
                int currentStaffId = await GetCurrentUserStaffIdAsync();

                var model = await _orderService.GetCreateOrderViewModelAsync(currentStaffId);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create order form");
                TempData["Error"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Order/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> Create(CreateOrderViewModel model, string itemsJson)
        {
            try
            {

                Console.WriteLine("=== CONTROLLER CREATE ACTION ===");
                Console.WriteLine($"itemsJson received: {itemsJson}");
                Console.WriteLine($"model.Items before deserialization: {model.Items?.Count ?? 0} items");

                // Deserialize items from JSON
                if (!string.IsNullOrEmpty(itemsJson))
                {
                    model.Items = JsonSerializer.Deserialize<List<OrderItemInputViewModel>>(itemsJson, 
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<OrderItemInputViewModel>();

                    Console.WriteLine($"model.Items after deserialization: {model.Items?.Count ?? 0} items");

                    if (model.Items != null)
                    {
                        foreach (var item in model.Items)
                        {
                            Console.WriteLine($"  Item: ProductId={item.ProductId}, Quantity={item.Quantity}");
                        }
                    }
                }

                // Validate that we have at least one item
                if (model.Items == null || !model.Items.Any())
                {
                    ModelState.AddModelError("", "Please add at least one product to the order.");
                }

                if (ModelState.IsValid)
                {
                    var result = await _orderService.CreateOrderAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "Order created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        Console.WriteLine("Service returned false");
                        ModelState.AddModelError("", "Failed to create order. Please try again.");
                    }
                }

                else
                {
                    Console.WriteLine("ModelState is INVALID:");
                    foreach (var key in ModelState.Keys)
                    {
                        var errors = ModelState[key].Errors;
                        if (errors.Any())
                        {
                            foreach (var error in errors)
                            {
                                Console.WriteLine($"  {key}: {error.ErrorMessage}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"EXCEPTION in Controller: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                _logger.LogError(ex, "Error creating order");
                ModelState.AddModelError("", "An error occurred while creating the order.");
            }

            // Reload dropdowns if validation fails
            int currentStaffId = await GetCurrentUserStaffIdAsync();
            var viewModel = await _orderService.GetCreateOrderViewModelAsync(currentStaffId);
            model.Customers = viewModel.Customers;
            model.Staff = viewModel.Staff;
            model.Shippers = viewModel.Shippers;
            model.Products = viewModel.Products;
            model.Categories = viewModel.Categories; // NEW: ensure categories are returned to view when validation fails

            return View(model);
        }

        // New: AJAX endpoint to fetch products for a chosen category
        [HttpGet]
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _orderService.GetProductsByCategoryAsync(categoryId);
                return Json(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products for category {CategoryId}", categoryId);
                return Json(new List<SelectListItem>());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> UpdateStatus(int orderId, int status)
        {
            try
            {
                var orderStatus = (OrderStatus)status;
                var result = await _orderService.UpdateOrderStatusAsync(orderId, orderStatus);

                if (result)
                {
                    TempData["Success"] = $"Order status updated to {orderStatus}";
                }
                else
                {
                    TempData["Error"] = "Failed to update order status";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order status for order ID: {OrderId}", orderId);
                TempData["Error"] = "An error occurred while updating the order status";
            }

            return RedirectToAction("Details", new { id = orderId });
        }

        // GET: Order/Delete/5
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var order = await _orderService.GetOrderByIdAsync(id.Value);
                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete confirmation for order ID: {OrderId}", id);
                TempData["Error"] = "An error occurred while loading the order.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Order/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _orderService.DeleteOrderAsync(id);
                if (result)
                {
                    TempData["Success"] = "Order deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete order.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order ID: {OrderId}", id);
                TempData["Error"] = "An error occurred while deleting the order.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<int> GetCurrentUserStaffIdAsync() 
        {
            var userEmail = User.Identity?.Name;

            //if (string.IsNullOrEmpty(userEmail))
            //    return null;

            var user = await _userManager.FindByEmailAsync(userEmail);

            return (int)user.StaffId;
        }

        // GET: Order/Edit/5
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var order = await _orderService.GetEditOrderViewModelAsync(id.Value);
                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for order ID: {OrderId}", id);
                TempData["Error"] = "An error occurred while loading the order.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Order/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> Edit(int id, EditOrderViewModel model, string itemsJson)
        {
            if (id != model.OrderId)
            {
                return NotFound();
            }

            try
            {
                Console.WriteLine("=== EDIT ORDER ACTION ===");
                Console.WriteLine($"Order ID: {id}");
                Console.WriteLine($"itemsJson received: {itemsJson}");

                // Deserialize new items from JSON
                if (!string.IsNullOrEmpty(itemsJson))
                {
                    model.NewItems = JsonSerializer.Deserialize<List<OrderItemInputViewModel>>(itemsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<OrderItemInputViewModel>();
                    Console.WriteLine($"New items after deserialization: {model.NewItems?.Count ?? 0}");
                }

                if (ModelState.IsValid)
                {
                    var result = await _orderService.UpdateOrderAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "Order updated successfully!";
                        return RedirectToAction(nameof(Details), new { id = model.OrderId });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update order. Please try again.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating order ID: {OrderId}", id);
                ModelState.AddModelError("", "An error occurred while updating the order.");
            }

            // Reload dropdowns if validation fails
            var viewModel = await _orderService.GetEditOrderViewModelAsync(id);
            if (viewModel != null)
            {
                model.Customers = viewModel.Customers;
                model.Staff = viewModel.Staff;
                model.Shippers = viewModel.Shippers;
                model.Products = viewModel.Products;
                model.ExistingItems = viewModel.ExistingItems;
            }

            return View(model);
        }

        // POST: Order/DeleteOrderDetail
        [HttpPost]
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> DeleteOrderDetail(int orderDetailId, int orderId)
        {
            try
            {
                var result = await _orderService.DeleteOrderDetailAsync(orderDetailId);
                if (result)
                {
                    TempData["Success"] = "Item removed from order successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to remove item from order.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting order detail ID: {OrderDetailId}", orderDetailId);
                TempData["Error"] = "An error occurred while removing the item.";
            }

            return RedirectToAction(nameof(Edit), new { id = orderId });
        }
    }
}