using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Data;
using StoreManagement.Models.Entities;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;

namespace StoreManagement.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;
        private readonly ApplicationDbContext _context;

        public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger, ApplicationDbContext context)
        {
            _customerService = customerService;
            _logger = logger;
            _context = context;
        }

        // GET: Customer
        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                TempData["Error"] = "An error occurred while loading customers.";
                return View(new List<CustomerViewModel>());
            }
        }

        // GET: Customer/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id.Value);
                if (customer == null)
                {
                    return NotFound();
                }

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer details for ID: {CustomerId}", id);
                TempData["Error"] = "An error occurred while loading customer details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Customer/Create
        [Authorize(Roles = "Admin,SalesStaff")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Customer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> Create(CustomerViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _customerService.CreateCustomerAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "Customer created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create customer. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating customer");
                    ModelState.AddModelError("", "An error occurred while creating the customer.");
                }
            }

            return View(model);
        }

        // GET: Customer/Edit/5
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id.Value);
                if (customer == null)
                {
                    return NotFound();
                }

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for customer ID: {CustomerId}", id);
                TempData["Error"] = "An error occurred while loading the customer.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Customer/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> Edit(int id, CustomerViewModel model)
        {
            if (id != model.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _customerService.UpdateCustomerAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "Customer updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update customer. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating customer ID: {CustomerId}", id);
                    ModelState.AddModelError("", "An error occurred while updating the customer.");
                }
            }

            return View(model);
        }

        // GET: Customer/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id.Value);
                if (customer == null)
                {
                    return NotFound();
                }

                // Check if customer has orders
                var hasOrders = await _customerService.HasOrdersAsync(id.Value);
                if (hasOrders)
                {
                    TempData["Error"] = "Cannot delete this customer because they have associated orders.";
                    return RedirectToAction(nameof(Index));
                }

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete confirmation for customer ID: {CustomerId}", id);
                TempData["Error"] = "An error occurred while loading the customer.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Customer/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);
                if (result)
                {
                    TempData["Success"] = "Customer deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete customer. They may have related orders.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer ID: {CustomerId}", id);
                TempData["Error"] = "An error occurred while deleting the customer.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SalesStaff")]
        public async Task<IActionResult> QuickCreate([FromBody] QuickCustomerCreateViewModel model)
        {
            if (string.IsNullOrEmpty(model.CustomerName))
            {
                return Json(new { success = false, message = "Customer name is required" });
            }

            try
            {
                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    ContactName = model.ContactName,
                    Address = model.Address,
                    City = model.City,
                    Country = model.Country,
                    Created = DateTime.Now
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    customerId = customer.CustomerId,
                    customerName = customer.CustomerName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating quick customer");
                return Json(new { success = false, message = "An error occurred while creating the customer" });
            }
        }
    }
}