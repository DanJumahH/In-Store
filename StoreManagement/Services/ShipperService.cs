using Microsoft.EntityFrameworkCore;
using StoreManagement.Data;
using StoreManagement.Models.Entities;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;

namespace StoreManagement.Services
{
    public class ShipperService : IShipperService
    {
        private readonly ApplicationDbContext _context;

        public ShipperService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ShipperViewModel>> GetAllShippersAsync()
        {
            return await _context.Shippers
                .AsNoTracking()
                .Select(s => new ShipperViewModel
                {
                    ShipperId = s.ShipperId,
                    ShipperName = s.ShipperName,
                    Phone = s.Phone,
                    OrderCount = s.Orders.Count()
                })
                .OrderBy(s => s.ShipperName)
                .ToListAsync();
        }

        public async Task<ShipperViewModel?> GetShipperByIdAsync(int id)
        {
            var shipper = await _context.Shippers
                .AsNoTracking()
                .Where(s => s.ShipperId == id)
                .Select(s => new ShipperViewModel
                {
                    ShipperId = s.ShipperId,
                    ShipperName = s.ShipperName,
                    Phone = s.Phone,
                    OrderCount = s.Orders.Count()
                })
                .FirstOrDefaultAsync();

            return shipper;
        }

        public async Task<bool> CreateShipperAsync(ShipperViewModel model)
        {
            try
            {
                var shipper = new Shipper
                {
                    ShipperName = model.ShipperName,
                    Phone = model.Phone
                };

                _context.Shippers.Add(shipper);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateShipperAsync(ShipperViewModel model)
        {
            try
            {
                var shipper = await _context.Shippers
                    .FirstOrDefaultAsync(s => s.ShipperId == model.ShipperId);

                if (shipper == null)
                    return false;

                shipper.ShipperName = model.ShipperName;
                shipper.Phone = model.Phone;

                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteShipperAsync(int id)
        {
            try
            {
                var shipper = await _context.Shippers.FindAsync(id);
                if (shipper == null)
                    return false;

                // Check if shipper has orders
                var hasOrders = await HasOrdersAsync(id);
                if (hasOrders)
                    return false;

                _context.Shippers.Remove(shipper);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ShipperExistsAsync(int id)
        {
            return await _context.Shippers.AnyAsync(s => s.ShipperId == id);
        }

        public async Task<bool> HasOrdersAsync(int id)
        {
            return await _context.Orders.AnyAsync(o => o.ShipperId == id);
        }
    }
}