using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;

namespace StoreManagement.Controllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        private readonly ISupplierService _supplierService;
        private readonly ILogger<SupplierController> _logger;

        public SupplierController(ISupplierService supplierService, ILogger<SupplierController> logger)
        {
            _supplierService = supplierService;
            _logger = logger;
        }

        // GET: Supplier
        public async Task<IActionResult> Index()
        {
            try
            {
                var suppliers = await _supplierService.GetAllSuppliersAsync();
                return View(suppliers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving suppliers");
                TempData["Error"] = "An error occurred while loading suppliers.";
                return View(new List<SupplierViewModel>());
            }
        }

        // GET: Supplier/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id.Value);
                if (supplier == null)
                {
                    return NotFound();
                }

                return View(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving supplier details for ID: {SupplierId}", id);
                TempData["Error"] = "An error occurred while loading supplier details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Supplier/Create
        [Authorize(Roles = "Admin,InventoryManager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Supplier/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Create(SupplierViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _supplierService.CreateSupplierAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "Supplier created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create supplier. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating supplier");
                    ModelState.AddModelError("", "An error occurred while creating the supplier.");
                }
            }

            return View(model);
        }

        // GET: Supplier/Edit/5
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id.Value);
                if (supplier == null)
                {
                    return NotFound();
                }

                return View(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for supplier ID: {SupplierId}", id);
                TempData["Error"] = "An error occurred while loading the supplier.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Supplier/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Edit(int id, SupplierViewModel model)
        {
            if (id != model.SupplierId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _supplierService.UpdateSupplierAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "Supplier updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update supplier. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating supplier ID: {SupplierId}", id);
                    ModelState.AddModelError("", "An error occurred while updating the supplier.");
                }
            }

            return View(model);
        }

        // GET: Supplier/Delete/5
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var supplier = await _supplierService.GetSupplierByIdAsync(id.Value);
                if (supplier == null)
                {
                    return NotFound();
                }

                // Check if supplier has products
                var hasProducts = await _supplierService.HasProductsAsync(id.Value);
                if (hasProducts)
                {
                    TempData["Error"] = "Cannot delete this supplier because it has associated products.";
                    return RedirectToAction(nameof(Index));
                }

                return View(supplier);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete confirmation for supplier ID: {SupplierId}", id);
                TempData["Error"] = "An error occurred while loading the supplier.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Supplier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _supplierService.DeleteSupplierAsync(id);
                if (result)
                {
                    TempData["Success"] = "Supplier deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete supplier. It may have related products.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting supplier ID: {SupplierId}", id);
                TempData["Error"] = "An error occurred while deleting the supplier.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
