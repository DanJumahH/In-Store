using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;

namespace StoreManagement.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;
        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        // GET: Category
        public async Task<IActionResult> Index()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving categories");
                TempData["Error"] = "An error occurred while loading categories.";
                return View(new List<CategoryViewModel>());
            }
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id.Value);
                if (category == null)
                {
                    return NotFound();
                }

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving category details for ID: {CategoryId}", id);
                TempData["Error"] = "An error occurred while loading category details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Category/Create
        [Authorize(Roles = "Admin,InventoryManager")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _categoryService.CreateCategoryAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "Category created successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to create category. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating category");
                    ModelState.AddModelError("", "An error occurred while creating the category.");
                }
            }

            return View(model);
        }

        // GET: Category/Edit/5
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id.Value);
                if (category == null)
                {
                    return NotFound();
                }

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for category ID: {CategoryId}", id);
                TempData["Error"] = "An error occurred while loading the category.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (id != model.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _categoryService.UpdateCategoryAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "Category updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update category. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating category ID: {CategoryId}", id);
                    ModelState.AddModelError("", "An error occurred while updating the category.");
                }
            }

            return View(model);
        }

        // GET: Category/Delete/5
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id.Value);
                if (category == null)
                {
                    return NotFound();
                }

                // Check if category has products
                var hasProducts = await _categoryService.HasProductsAsync(id.Value);
                if (hasProducts)
                {
                    TempData["Error"] = "Cannot delete this category because it has associated products.";
                    return RedirectToAction(nameof(Index));
                }

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete confirmation for category ID: {CategoryId}", id);
                TempData["Error"] = "An error occurred while loading the category.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,InventoryManager")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);
                if (result)
                {
                    TempData["Success"] = "Category deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete category. It may have related products.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category ID: {CategoryId}", id);
                TempData["Error"] = "An error occurred while deleting the category.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
