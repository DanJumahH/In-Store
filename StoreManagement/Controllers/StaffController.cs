using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;

namespace StoreManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StaffController : Controller
    {
        private readonly IStaffService _staffService;
        private readonly ILogger<StaffController> _logger;

        public StaffController(IStaffService staffService, ILogger<StaffController> logger)
        {
            _staffService = staffService;
            _logger = logger;
        }

        // GET: Staff
        public async Task<IActionResult> Index()
        {
            try
            {
                var staff = await _staffService.GetAllStaffAsync();
                return View(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff");
                TempData["Error"] = "An error occurred while loading staff.";
                return View(new List<StaffViewModel>());
            }
        }

        // GET: Staff/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var staff = await _staffService.GetStaffByIdAsync(id.Value);
                if (staff == null)
                {
                    return NotFound();
                }

                return View(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving staff details for ID: {StaffId}", id);
                TempData["Error"] = "An error occurred while loading staff details.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Staff/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var model = await _staffService.GetStaffForCreateAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create staff form");
                TempData["Error"] = "An error occurred while loading the form.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Staff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _staffService.CreateStaffAsync(model);

                    if (result.Success)
                    {
                        // If app user was created, show credentials
                        if (result.AppUserCreated && !string.IsNullOrEmpty(result.GeneratedPassword))
                        {
                            TempData["Success"] = result.Message;
                            TempData["AppUserCreated"] = true;
                            TempData["Email"] = result.Email;
                            TempData["Password"] = result.GeneratedPassword;
                            TempData["Role"] = result.Role;

                            return RedirectToAction(nameof(ShowCredentials), new { staffId = result.StaffId });
                        }
                        else
                        {
                            TempData["Success"] = result.Message;
                            return RedirectToAction(nameof(Index));
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", result.Message);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating staff");
                    ModelState.AddModelError("", "An error occurred while creating the staff member.");
                }
            }

            // Reload roles if validation fails
            model = await _staffService.GetStaffForCreateAsync();
            model.FirstName = model.FirstName;
            model.LastName = model.LastName;
            model.Email = model.Email;
            model.StaffRole = model.StaffRole;

            return View(model);
        }

        // GET: Staff/ShowCredentials
        public IActionResult ShowCredentials(int staffId)
        {
            if (TempData["AppUserCreated"] == null)
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.StaffId = staffId;
            ViewBag.Email = TempData["Email"]?.ToString();
            ViewBag.Password = TempData["Password"]?.ToString();
            ViewBag.Role = TempData["Role"]?.ToString();
            ViewBag.SuccessMessage = TempData["Success"]?.ToString();

            return View();
        }

        // GET: Staff/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var staff = await _staffService.GetStaffForEditAsync(id.Value);
                if (staff == null)
                {
                    return NotFound();
                }

                return View(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading edit form for staff ID: {StaffId}", id);
                TempData["Error"] = "An error occurred while loading the staff member.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Staff/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StaffViewModel model)
        {
            if (id != model.StaffId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _staffService.UpdateStaffAsync(model);
                    if (result)
                    {
                        TempData["Success"] = "Staff member updated successfully!";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Failed to update staff member. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating staff ID: {StaffId}", id);
                    ModelState.AddModelError("", "An error occurred while updating the staff member.");
                }
            }

            // Reload roles if validation fails
            var staffWithRoles = await _staffService.GetStaffForEditAsync(id);
            model.AvailableRoles = staffWithRoles.AvailableRoles;

            return View(model);
        }

        // GET: Staff/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var staff = await _staffService.GetStaffByIdAsync(id.Value);
                if (staff == null)
                {
                    return NotFound();
                }

                // Check if staff has orders
                var hasOrders = await _staffService.HasOrdersAsync(id.Value);
                if (hasOrders)
                {
                    TempData["Error"] = "Cannot delete this staff member because they have associated orders.";
                    return RedirectToAction(nameof(Index));
                }

                return View(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading delete confirmation for staff ID: {StaffId}", id);
                TempData["Error"] = "An error occurred while loading the staff member.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Staff/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _staffService.DeleteStaffAsync(id);
                if (result)
                {
                    TempData["Success"] = "Staff member deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Failed to delete staff member. They may have related orders.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting staff ID: {StaffId}", id);
                TempData["Error"] = "An error occurred while deleting the staff member.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}