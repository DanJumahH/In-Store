using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagement.Data;
using StoreManagement.Models;
using StoreManagement.Models.Entities;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;

namespace StoreManagement.Services
{
    public class StaffService : IStaffService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // Define app user roles
        private readonly string[] _appUserRoles = { "InventoryManager", "SalesStaff" };

        public StaffService(
            ApplicationDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IEnumerable<StaffViewModel>> GetAllStaffAsync()
        {
            return await _context.Staffs
                .AsNoTracking()
                .Select(s => new StaffViewModel
                {
                    StaffId = s.StaffId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    StaffRole = s.StaffRole ?? "N/A",
                    BirthDate = s.BirthDate,
                    IsAppUser = _appUserRoles.Contains(s.StaffRole ?? ""),
                    OrderCount = s.Orders.Count()
                })
                .OrderBy(s => s.LastName)
                .ThenBy(s => s.FirstName)
                .ToListAsync();
        }

        public async Task<StaffViewModel?> GetStaffByIdAsync(int id)
        {
            var staff = await _context.Staffs
                .AsNoTracking()
                .Where(s => s.StaffId == id)
                .Select(s => new StaffViewModel
                {
                    StaffId = s.StaffId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    StaffRole = s.StaffRole ?? "",
                    BirthDate = s.BirthDate,
                    Notes = s.Notes,
                    IsAppUser = _appUserRoles.Contains(s.StaffRole ?? ""),
                    OrderCount = s.Orders.Count()
                })
                .FirstOrDefaultAsync();

            return staff;
        }

        public async Task<StaffViewModel> GetStaffForCreateAsync()
        {
            var roles = new List<SelectListItem>
            {
                //new SelectListItem { Value = "Admin", Text = "Admin (App User)" },
                new SelectListItem { Value = "InventoryManager", Text = "Inventory Manager (App User)" },
                new SelectListItem { Value = "SalesStaff", Text = "Sales Staff (App User)" },
                new SelectListItem { Value = "Cashier", Text = "Cashier" },
                new SelectListItem { Value = "WarehouseStaff", Text = "Warehouse Staff" },
                new SelectListItem { Value = "Supervisor", Text = "Supervisor" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };

            return new StaffViewModel
            {
                AvailableRoles = roles
            };
        }

        public async Task<StaffViewModel> GetStaffForEditAsync(int id)
        {
            var staff = await GetStaffByIdAsync(id);
            if (staff == null)
                return null!;

            var roles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "Admin (App User)" },
                new SelectListItem { Value = "InventoryManager", Text = "Inventory Manager (App User)" },
                new SelectListItem { Value = "SalesStaff", Text = "Sales Staff (App User)" },
                new SelectListItem { Value = "Cashier", Text = "Cashier" },
                new SelectListItem { Value = "WarehouseStaff", Text = "Warehouse Staff" },
                new SelectListItem { Value = "Supervisor", Text = "Supervisor" },
                new SelectListItem { Value = "Other", Text = "Other" }
            };

            staff.AvailableRoles = roles;
            return staff;
        }

        public async Task<StaffCreateResultViewModel> CreateStaffAsync(StaffViewModel model)
        {
            var result = new StaffCreateResultViewModel();

            try
            {
                // Check if role is an app user role
                bool isAppUserRole = await IsAppUserRole(model.StaffRole);

                // Validate email if app user role
                if (isAppUserRole && string.IsNullOrEmpty(model.Email))
                {
                    result.Success = false;
                    result.Message = "Email is required for app user roles (InventoryManager, SalesStaff)";
                    return result;
                }

                // Create staff record
                var staff = new Staff
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    BirthDate = model.BirthDate,
                    Email = model.Email,
                    StaffRole = model.StaffRole,
                    Notes = model.Notes
                };

                _context.Staffs.Add(staff);
                await _context.SaveChangesAsync();

                result.StaffId = staff.StaffId;
                result.Success = true;
                result.Message = "Staff member created successfully!";

                // If app user role, create AppUser account
                if (isAppUserRole && !string.IsNullOrEmpty(model.Email))
                {
                    // Generate temporary password
                    string tempPassword = GenerateTemporaryPassword();

                    // Create AppUser
                    var appUser = new AppUser
                    {
                        UserName = model.Email,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        StaffId = staff.StaffId,
                        EmailConfirmed = true // Skip email confirmation for now
                    };

                    var createResult = await _userManager.CreateAsync(appUser, tempPassword);

                    if (createResult.Succeeded)
                    {
                        // Assign role to user
                        await _userManager.AddToRoleAsync(appUser, model.StaffRole);

                        result.AppUserCreated = true;
                        result.GeneratedPassword = tempPassword;
                        result.Email = model.Email;
                        result.Role = model.StaffRole;
                        result.Message = "Staff member and app user account created successfully!";
                    }
                    else
                    {
                        result.AppUserCreated = false;
                        result.Message = $"Staff created, but failed to create app user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}";
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error creating staff: {ex.Message}";
                return result;
            }
        }

        public async Task<bool> UpdateStaffAsync(StaffViewModel model)
        {
            try
            {
                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(s => s.StaffId == model.StaffId);

                if (staff == null)
                    return false;

                staff.FirstName = model.FirstName;
                staff.LastName = model.LastName;
                staff.BirthDate = model.BirthDate;
                staff.Email = model.Email;
                staff.StaffRole = model.StaffRole;
                staff.Notes = model.Notes;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteStaffAsync(int id)
        {
            try
            {
                var staff = await _context.Staffs.FindAsync(id);
                if (staff == null)
                    return false;

                // Check if staff has orders
                var hasOrders = await HasOrdersAsync(id);
                if (hasOrders)
                    return false;

                // Check if staff has linked AppUser
                var appUser = await _context.Users.FirstOrDefaultAsync(u => u.StaffId == id);
                if (appUser != null)
                {
                    // Delete AppUser first
                    await _userManager.DeleteAsync(appUser);
                }

                _context.Staffs.Remove(staff);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> StaffExistsAsync(int id)
        {
            return await _context.Staffs.AnyAsync(s => s.StaffId == id);
        }

        public async Task<bool> HasOrdersAsync(int id)
        {
            return await _context.Orders.AnyAsync(o => o.EmployeeId == id);
        }

        public async Task<bool> IsAppUserRole(string role)
        {
            return await Task.FromResult(_appUserRoles.Contains(role));
        }

        private string GenerateTemporaryPassword()
        {
            // Generate a secure temporary password
            // Format: Staff + 4 random digits + special char
            var random = new Random();
            int randomNumber = random.Next(1000, 9999);
            char specialChar = "!@#$%".ToCharArray()[random.Next(5)];

            return $"Staff{randomNumber}{specialChar}";
        }
    }
}