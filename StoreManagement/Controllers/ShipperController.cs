using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;

namespace StoreManagement.Controllers
{
    [Authorize]
    public class ShipperController : Controller
    {
        private readonly IShipperService _shipperService;
        private readonly ILogger<ShipperController> _logger;

        public ShipperController(IShipperService shipperService, ILogger<ShipperController> logger)
        {
            _shipperService = shipperService;
            _logger = logger;
        }

        // GET: Shipper
        public async Task<IActionResult> Index()
        {
            try
            {
                var shippers = await _shipperService.GetAllShippersAsync();
                return View(shippers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shippers");
                TempData["Error"] = "An error occurred while loading shippers.";
                return View(new List<ShipperViewModel>());
            }
        }

        // GET: Shipper/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var shipper = await _shipperService.GetShipperByIdAsync(id.Value);
                if (shipper == null)
                {
                    return NotFound();
                }

                return View(shipper);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving shipper details for ID: {ShipperId}", id);
                TempData["Error"] = "An error occurred while loading shipper details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Shipper/Create
        [Authorize(Roles = "Admin,InventoryManager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Shipper/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Create(ShipperViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _shipperService.CreateShipperAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "Shipper created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create shipper. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating shipper");
                    ModelState.AddModelError("", "An error occurred while creating the shipper.");
                }
            }

            return View(model);
        }

        // GET: Shipper/Edit/5
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var shipper = await _shipperService.GetShipperByIdAsync(id.Value);
                if (shipper == null)
                {
                    return NotFound();
                }

                return View(shipper);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for shipper ID: {ShipperId}", id);
                TempData["Error"] = "An error occurred while loading the shipper.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Shipper/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Edit(int id, ShipperViewModel model)
        {
            if (id != model.ShipperId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _shipperService.UpdateShipperAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "Shipper updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update shipper. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating shipper ID: {ShipperId}", id);
                    ModelState.AddModelError("", "An error occurred while updating the shipper.");
                }
            }

            return View(model);
        }

        // GET: Shipper/Delete/5
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var shipper = await _shipperService.GetShipperByIdAsync(id.Value);
                if (shipper == null)
                {
                    return NotFound();
                }

                // Check if shipper has orders
                var hasOrders = await _shipperService.HasOrdersAsync(id.Value);
                if (hasOrders)
                {
                    TempData["Error"] = "Cannot delete this shipper because they have associated orders.";
                    return RedirectToAction(nameof(Index));
                }

                return View(shipper);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete confirmation for shipper ID: {ShipperId}", id);
                TempData["Error"] = "An error occurred while loading the shipper.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Shipper/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _shipperService.DeleteShipperAsync(id);
                if (result)
                {
                    TempData["Success"] = "Shipper deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete shipper. They may have related orders.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting shipper ID: {ShipperId}", id);
                TempData["Error"] = "An error occurred while deleting the shipper.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}