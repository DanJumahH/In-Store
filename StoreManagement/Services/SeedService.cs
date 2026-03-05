using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Data;
using StoreManagement.Models;
using StoreManagement.Models.Entities;

namespace StoreManagement.Services
{
    public class SeedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SeedService> _logger;

        public SeedService(IServiceProvider serviceProvider, ILogger<SeedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Database Seeding ...");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            try
            {
                _logger.LogInformation("Ensuring Database is created...");
                await context.Database.MigrateAsync();

                _logger.LogInformation("Seeding roles ...");
                await EnsureRoleExists(roleManager, "Admin");
                await EnsureRoleExists(roleManager, "InventoryManager");
                await EnsureRoleExists(roleManager, "SalesStaff");

                await EnsureAdminUser(userManager);

                await EnsureInventoryManagerUser(context, userManager);

                await EnsureStaffUser(context, userManager);

                _logger.LogInformation("Database Seeding completed Successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;


        // Role method
        private static async Task EnsureRoleExists(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

        }

        private async Task EnsureAdminUser(UserManager<AppUser> userManager)
        {
            // Default Admin User
            _logger.LogInformation("Seeding default admin user ...");
            var adminEmail = "admin@store.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new AppUser
                {
                    FirstName = "System",
                    LastName = "Admin",
                    UserName = adminEmail,
                    NormalizedUserName = adminEmail.ToUpper(),
                    Email = adminEmail,
                    NormalizedEmail = adminEmail.ToUpper(),
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    _logger.LogInformation("Assigning Admin role to Admin user");
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                else
                {
                    throw new Exception($"Failed to Create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

        }

        private async Task EnsureInventoryManagerUser(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            // Inventory Manager
            _logger.LogInformation("Seeding default Inventory Manager...");
            var inventoryEmail = "inventory@store.com";
            var inventoryStaff = await context.Staffs.FirstOrDefaultAsync(s => s.Email == inventoryEmail);
            if (inventoryStaff == null)
            {
                inventoryStaff = new Staff
                {
                    FirstName = "Default",
                    LastName = "Inventory Manager",
                    Email = inventoryEmail,
                    Notes = "Auto-generated inventory manager staff."
                };
                context.Staffs.Add(inventoryStaff);
                await context.SaveChangesAsync();
            }
            if (await userManager.FindByEmailAsync(inventoryEmail) == null)
            {
                var inventoryUser = new AppUser
                {
                    FirstName = inventoryStaff.FirstName,
                    LastName = inventoryStaff.LastName,
                    UserName = inventoryEmail,
                    Email = inventoryEmail,
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    StaffId = inventoryStaff.StaffId
                };
                var result = await userManager.CreateAsync(inventoryUser, "Inventory@123");
                if (result.Succeeded)
                {
                    _logger.LogInformation("Assigning InventoryManager role to Inventory user");
                    await userManager.AddToRoleAsync(inventoryUser, "InventoryManager");
                }
                else
                {
                    throw new Exception($"Failed to create inventory user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private async Task EnsureStaffUser(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            // set existing staffs as default users
            _logger.LogInformation("Updating Existing staff with placeholder Emails...");
            var existingStaff = await context.Staffs
                .Where(s => string.IsNullOrEmpty(s.Email))
                .ToListAsync();

            foreach (var staff in existingStaff) { staff.Email = $"staff{staff.StaffId}@store.local"; }
            await context.SaveChangesAsync();

            _logger.LogInformation("Creating Identity user for Existing staffs...");
            foreach (var staff in await context.Staffs.ToListAsync())
            {
                if (string.IsNullOrEmpty(staff.Email)) continue;
                var existingUser = await userManager.FindByEmailAsync(staff.Email);
                if (existingUser == null)
                {
                    var user = new AppUser
                    {
                        UserName = staff.Email,
                        Email = staff.Email,
                        EmailConfirmed = false,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        StaffId = staff.StaffId,
                        FirstName = staff.FirstName,
                        LastName = staff.LastName
                    };
                    var result = await userManager.CreateAsync(user, "Store@123");
                    if (result.Succeeded)
                    {
                        _logger.LogInformation("Assigning SalesStaff role to staff user...");
                        await userManager.AddToRoleAsync(user, "SalesStaff");
                    }
                    else
                    {
                        throw new Exception($"Failed to create staff user role: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }
    }
}
