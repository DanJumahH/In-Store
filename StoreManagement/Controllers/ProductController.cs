using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;

namespace StoreManagement.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        // GET: /Product
        public async Task<IActionResult> Index(string? search, int? categoryId, int? supplierId, string? sort)
        {
            try
            {
                var products = await _productService.GetProductsAsync(search, categoryId, supplierId, sort);

                var (categories, suppliers) = await _productService.GetFilterListsAsync();
                ViewBag.Categories = categories;
                ViewBag.Suppliers = suppliers;

                ViewData["CurrentSearch"] = search;
                ViewData["CurrentCategoryId"] = categoryId;
                ViewData["CurrentSupplierId"] = supplierId;
                ViewData["CurrentSort"] = sort;

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                TempData["Error"] = "An error occurred while loading products.";
                return View(new List<ProductListViewModel>());
            }
        }

        // GET: Product/Details/5
        public async Task<IActionResult> Details(int? id, string? returnUrl)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var product = await _productService.GetProductByIdAsync(id.Value);
                if (product == null)
                {
                    return NotFound();
                }

                // keep returnUrl for the view so Back returns to the filtered list
                ViewData["ReturnUrl"] = returnUrl;

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product details for ID: {ProductId}", id);
                TempData["Error"] = "An error occurred while loading product details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Product/Create
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = await _productService.GetProductForCreateAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create product form");
                TempData["Error"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Create(ProductViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _productService.CreateProductAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "Product created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create product. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product");
                    ModelState.AddModelError("", "An error occurred while creating the product.");
                }
            }

            // Reload dropdowns if validation fails
            model = await _productService.GetProductForCreateAsync();
            model.ProductName = model.ProductName;
            model.SupplierId = model.SupplierId;
            model.CategoryId = model.CategoryId;
            model.Unit = model.Unit;
            model.Price = model.Price;

            return View(model);
        }

        // GET: Product/Edit/5
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var product = await _productService.GetProductForEditAsync(id.Value);
                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for product ID: {ProductId}", id);
                TempData["Error"] = "An error occurred while loading the product.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Product/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Edit(int id, ProductViewModel model)
        {
            if (id != model.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _productService.UpdateProductAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "Product updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update product. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating product ID: {ProductId}", id);
                    ModelState.AddModelError("", "An error occurred while updating the product.");
                }
            }

            // Reload dropdowns if validation fails
            var productWithDropdowns = await _productService.GetProductForEditAsync(id);
            model.Categories = productWithDropdowns.Categories;
            model.Suppliers = productWithDropdowns.Suppliers;

            return View(model);
        }

        // GET: Product/Delete/5
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var product = await _productService.GetProductByIdAsync(id.Value);
                if (product == null)
                {
                    return NotFound();
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete confirmation for product ID: {ProductId}", id);
                TempData["Error"] = "An error occurred while loading the product.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Product/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (result)
                {
                    TempData["Success"] = "Product deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete product. It may have related orders.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product ID: {ProductId}", id);
                TempData["Error"] = "An error occurred while deleting the product.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
