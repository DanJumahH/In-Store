using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using StoreManagement.Models;
using StoreManagement.Models.Enums;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Data;
using Microsoft.Extensions.Options;
using StoreManagement.Models.Entities;

namespace StoreManagement.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly IPaystackService _paystackService;
        private readonly ApplicationDbContext _context;
        private readonly PaystackSettings _payOptions;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public OrderController(
            IOrderService orderService,
            ILogger<OrderController> logger,
            UserManager<AppUser> userManager,
            IPaystackService paystackService,
            ApplicationDbContext context,
            IOptions<PaystackSettings> payOptions,
            IEmailService emailService,
            IConfiguration configuration)
        {
            _orderService = orderService;
            _logger = logger;
            _userManager = userManager;
            _paystackService = paystackService;
            _context = context;
            _payOptions = payOptions.Value;
            _emailService = emailService;
            _configuration = configuration;
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
        // Receipt Action

        public async Task<IActionResult> Receipt(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Fetch the order to check payment status
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                {
                    return NotFound();
                }

                // Only show receipt if order is paid
                if (order.PaymentStatus != PaymentStatus.Paid)
                {
                    TempData["Error"] = "Receipt is only available for paid orders.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Get receipt data from service
                var model = await _orderService.GetReceiptAsync(id.Value);
                if (model == null)
                {
                    return NotFound();
                }


                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving receipt for order ID: {OrderId}", id);
                TempData["Error"] = "An error occurred while loading the receipt.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendReceipt(int id)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction(nameof(Receipt), new { id });
                }

                // Get receipt model
                var model = await _orderService.GetReceiptAsync(id);
                if (model == null)
                {
                    TempData["Error"] = "Could not generate receipt.";
                    return RedirectToAction(nameof(Receipt), new { id });
                }

                // Determine which email to use (customer email OR default from settings)
                var emailToUse = order.Customer?.Email ?? _configuration["Paystack:DefaultCustomerEmail"];

                // Check if we have ANY email to send to
                if (string.IsNullOrWhiteSpace(emailToUse))
                {
                    TempData["Error"] = "No email address configured. Please add a default email in settings.";
                    return RedirectToAction(nameof(Receipt), new { id });
                }

                // Send the receipt email
                var emailSent = await _emailService.SendReceiptEmailAsync(emailToUse, model);

                if (emailSent)
                {
                    TempData["Success"] = "Receipt sent successfully!";
                    _logger.LogInformation("Receipt email sent for order ID: {OrderId} to {Email}", id, emailToUse);
                }
                else
                {
                    TempData["Error"] = "Failed to send receipt email. Please try again.";
                    _logger.LogWarning("Failed to send receipt email for order ID: {OrderId}", id);
                }

                return RedirectToAction(nameof(Receipt), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending receipt for order ID: {OrderId}", id);
                TempData["Error"] = "An error occurred while sending the receipt.";
                return RedirectToAction(nameof(Receipt), new { id });
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

                // Validate we have at least one item
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
                        
                        // Get current staff ID
                        int thisStaffId = await GetCurrentUserStaffIdAsync();

                     
                        var lastOrder = await _context.Orders
                            .Where(o => o.EmployeeId == thisStaffId)
                            .OrderByDescending(o => o.OrderId)
                            .FirstOrDefaultAsync();
                        
                        if (lastOrder != null)
                        {
                            return RedirectToAction(nameof(Details), new { id = lastOrder.OrderId });
                        }
                        
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
                // Check if order exists and get payment status
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                {
                    TempData["Error"] = "Order not found.";
                    return RedirectToAction(nameof(Index));
                }

                // CRITICAL: Only allow status update if payment is confirmed
                if (order.PaymentStatus != PaymentStatus.Paid)
                {
                    TempData["Error"] = "Order status cannot be updated until payment is confirmed.";
                    return RedirectToAction(nameof(Details), new { id = orderId });
                }

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

        // POST: Order/Pay/{orderId}
        [HttpPost]
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> Pay(int id)
        {
            try
            {
                // Load order
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null)
                {
                    return NotFound();
                }

                // Check if already paid
                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    TempData["Error"] = "This order has already been paid.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Calculate total amount
                decimal totalAmount = order.OrderDetails.Sum(od => od.Product.Price * od.Quantity);

                if (totalAmount <= 0)
                {
                    TempData["Error"] = "Order total must be greater than zero.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                // Generate unique payment reference
                string paymentReference = $"ORD-{id}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";

                // Callback URL
                var callbackUrl = Url.Action(nameof(PaymentCallback), "Order", null, Request.Scheme);

                // Placeholder Customer Email
                var customerEmail = _payOptions.DefaultCustomerEmail;

                // Initialize payment with Paystack
                var initResponse = await _paystackService.InitializePaymentAsync(
                    customerEmail,
                    totalAmount,
                    paymentReference,
                    callbackUrl
                );

                if (!initResponse.Status)
                {
                    TempData["Error"] = "Failed to initialize payment. Please try again.";
                    return RedirectToAction(nameof(Details), new { id});
                }

                // Save reference and mark as pending payment
                order.PaymentReference = paymentReference;
                order.PaymentMethod = PaymentMethod.Paystack;
                order.PaymentStatus = PaymentStatus.Pending;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                // Redirect to Paystack payment page
                return Redirect(initResponse.AuthorizationUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initiating payment for order ID: {OrderId}", id);
                TempData["Error"] = "An error occurred while processing payment.";
                return RedirectToAction(nameof(Details), new { id});
            }
        }

        // GET: Order/PaymentCallback
        [AllowAnonymous]
        public async Task<IActionResult> PaymentCallback(string reference)
        {
            try
            {
                if (string.IsNullOrEmpty(reference))
                {
                    return BadRequest("Payment reference is required.");
                }

                // Find order by payment reference
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.PaymentReference == reference);

                if (order == null)
                {
                    return NotFound("Order not found.");
                }

                // If already marked as paid, show success
                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    ViewBag.OrderId = order.OrderId;
                    ViewBag.Message = "Payment already confirmed.";
                    return View("PaymentSuccess");
                }

                // Verify payment with Paystack
                var verifyResponse = await _paystackService.VerifyPaymentAsync(reference);

                if (!verifyResponse.Status || verifyResponse.PaymentStatus != "success")
                {
                    order.PaymentStatus = PaymentStatus.Failed;
                    order.PaymentNotes = $"Payment was not completed: {verifyResponse.PaymentStatus}";
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();

                    ViewBag.OrderId = order.OrderId;
                    ViewBag.Message = verifyResponse.PaymentStatus;
                    return View("PaymentFailed");
                }

                // Verify amount matches
                decimal expectedAmount = order.OrderDetails.Sum(od => od.Product.Price * od.Quantity);
                int expectedAmountInKobo = (int)(expectedAmount * 100);

                if (verifyResponse.Amount != expectedAmountInKobo)
                {
                    order.PaymentStatus = PaymentStatus.Failed;
                    order.PaymentNotes = "Payment amount mismatch.";
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();

                    ViewBag.OrderId = order.OrderId;
                    ViewBag.Message = "Payment amount does not match order total.";
                    return View("PaymentFailed");
                }

                // Mark order as paid
                order.PaymentStatus = PaymentStatus.Paid;
                order.Status = OrderStatus.Paid;
                order.PaymentDate = DateTime.UtcNow;
                order.StatusUpdatedAt = DateTime.UtcNow;
                order.PaymentNotes = $"Paid via Paystack on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment successful for order {OrderId}, reference: {Reference}", order.OrderId, reference);

                ViewBag.OrderId = order.OrderId;
                ViewBag.Message = "Payment confirmed successfully!";
                return View("PaymentSuccess");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment callback for reference: {Reference}", reference);
                return StatusCode(500, "An error occurred while processing payment.");
            }
        }

        // POST: Order/MarkAsPaidManual
        [HttpPost]
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> MarkAsPaidManual(int id, string? paymentNotes)
        {
            try
            {
                var order = await _context.Orders.FindAsync(id);
                if (order == null)
                {
                    return NotFound();
                }

                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    TempData["Error"] = "Order is already marked as paid.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                order.PaymentStatus = PaymentStatus.Paid;
                order.PaymentMethod = PaymentMethod.Manual;
                order.PaymentDate = DateTime.UtcNow;
                order.Status = OrderStatus.Paid;
                order.StatusUpdatedAt = DateTime.UtcNow;
                order.PaymentNotes = paymentNotes ?? $"Manual payment received on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";

                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Order marked as paid successfully!";
                _logger.LogInformation("Order {OrderId} marked as paid manually by user", id);

                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking order {OrderId} as paid", id);
                TempData["Error"] = "An error occurred while processing payment.";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // POST: Order/PaystackWebhook
        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> PaystackWebhook()
        {
            try
            {
                // Read raw body for signature validation
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();

                // Get signature from header
                var signature = Request.Headers["x-paystack-signature"].ToString();

                if (string.IsNullOrEmpty(signature))
                {
                    _logger.LogWarning("Webhook received without signature");
                    return Unauthorized();
                }

                // Validate webhook signature
                if (!_paystackService.ValidateWebhookSignature(body, signature))
                {
                    _logger.LogWarning("Invalid webhook signature received");
                    return Unauthorized();
                }

                // Parse webhook payload
                using var jsonDoc = JsonDocument.Parse(body);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("event", out var eventProp))
                {
                    return Ok(); // Acknowledge receipt
                }

                string webhookEvent = eventProp.GetString() ?? string.Empty;

                // Only handle successful charge events
                if (webhookEvent != "charge.success")
                {
                    return Ok(); // Acknowledge but don't process
                }

                if (!root.TryGetProperty("data", out var data))
                {
                    return Ok();
                }

                string reference = data.GetProperty("reference").GetString() ?? string.Empty;

                if (string.IsNullOrEmpty(reference))
                {
                    return Ok();
                }

                // Find and update order
                _logger.LogInformation("Valid Webhook received for reference: {Reference}", reference);
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.PaymentReference == reference);

                if (order == null)
                {
                    _logger.LogWarning("Webhook received for unknown reference: {Reference}", reference);
                    return Ok(); // Still return 200 to prevent retries
                }

                // Skip if already paid
                if (order.PaymentStatus == PaymentStatus.Paid)
                {
                    _logger.LogInformation("Webhook skipped, order {OrderId} already paid", order.OrderId);
                    return Ok();
                }

                // Verify with Paystack
                var verifyResponse = await _paystackService.VerifyPaymentAsync(reference);

                if (verifyResponse.Status && verifyResponse.PaymentStatus == "success")
                {
                    order.PaymentStatus = PaymentStatus.Paid;
                    order.Status = OrderStatus.Paid;
                    order.PaymentDate = DateTime.UtcNow;
                    order.StatusUpdatedAt = DateTime.UtcNow;
                    order.PaymentNotes = $"Webhook confirmation on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                    _context.Orders.Update(order);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Webhook payment confirmed for order {OrderId}", order.OrderId);
                }

                // Always return 200 to acknowledge receipt
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook");
                return Ok(); // Return 200 even on error to prevent Paystack retries
            }
        }
    }
}