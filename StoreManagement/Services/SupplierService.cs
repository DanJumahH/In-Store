using Microsoft.EntityFrameworkCore;
using StoreManagement.Data;
using StoreManagement.Models.Entities;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;

namespace StoreManagement.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ApplicationDbContext _context;

        public SupplierService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<SupplierViewModel>> GetAllSuppliersAsync()
        {
            return await _context.Suppliers
                .AsNoTracking()
                .Select(s => new SupplierViewModel
                {
                    SupplierId = s.SupplierId,
                    SupplierName = s.SupplierName,
                    ContactName = s.ContactName,
                    Address = s.Address,
                    City = s.City,
                    PostalCode = s.PostalCode,
                    Country = s.Country,
                    Phone = s.Phone,
                    ProductCount = s.Products.Count()
                })
                .OrderBy(s => s.SupplierName)
                .ToListAsync();
        }

        public async Task<SupplierViewModel?> GetSupplierByIdAsync(int id)
        {
            var supplier = await _context.Suppliers
                .AsNoTracking()
                .Where(s => s.SupplierId == id)
                .Select(s => new SupplierViewModel
                {
                    SupplierId = s.SupplierId,
                    SupplierName = s.SupplierName,
                    ContactName = s.ContactName,
                    Address = s.Address,
                    City = s.City,
                    PostalCode = s.PostalCode,
                    Country = s.Country,
                    Phone = s.Phone,
                    ProductCount = s.Products.Count()
                })
                .FirstOrDefaultAsync();

            return supplier;
        }

        public async Task<bool> CreateSupplierAsync(SupplierViewModel model)
        {
            try
            {
                var supplier = new Supplier
                {
                    SupplierName = model.SupplierName,
                    ContactName = model.ContactName,
                    Address = model.Address,
                    City = model.City,
                    PostalCode = model.PostalCode,
                    Country = model.Country,
                    Phone = model.Phone
                };

                _context.Suppliers.Add(supplier);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateSupplierAsync(SupplierViewModel model)
        {
            try
            {
                var supplier = await _context.Suppliers
                    .FirstOrDefaultAsync(s => s.SupplierId == model.SupplierId);

                if (supplier == null)
                    return false;

                supplier.SupplierName = model.SupplierName;
                supplier.ContactName = model.ContactName;
                supplier.Address = model.Address;
                supplier.City = model.City;
                supplier.PostalCode = model.PostalCode;
                supplier.Country = model.Country;
                supplier.Phone = model.Phone;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            try
            {
                var supplier = await _context.Suppliers.FindAsync(id);
                if (supplier == null)
                    return false;

                // Check if supplier has products
                var hasProducts = await HasProductsAsync(id);
                if (hasProducts)
                    return false;

                _context.Suppliers.Remove(supplier);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SupplierExistsAsync(int id)
        {
            return await _context.Suppliers.AnyAsync(s => s.SupplierId == id);
        }

        public async Task<bool> HasProductsAsync(int id)
        {
            return await _context.Products.AnyAsync(p => p.SupplierId == id);
        }
    }
}
