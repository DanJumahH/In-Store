using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Data;
using StoreManagement.Models.Entities;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;

namespace StoreManagement.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _dbContext;

        public CustomerService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<CustomerViewModel>> GetAllCustomersAsync()
        {
            return await _dbContext.Customers
                .AsNoTracking()
                .Select(c => new CustomerViewModel
                {
                    CustomerId = c.CustomerId,
                    CustomerName = c.CustomerName,
                    ContactName = c.ContactName,
                    CustomerEmail = c.Email,
                    Address = c.Address,
                    City = c.City,
                    PostalCode = c.PostalCode,
                    Country = c.Country,
                    Created = c.Created,
                    OrderCount = c.Orders.Count()
                })
                .OrderBy(c => c.CustomerName)
                .ToListAsync();
        }

        public async Task<CustomerViewModel> GetCustomerByIdAsync(int id)
        {
            var customer = await _dbContext.Customers
                .AsNoTracking()
                .Where(c => c.CustomerId == id)
                .Select(c => new CustomerViewModel
                {
                    CustomerId = c.CustomerId,
                    CustomerName = c.CustomerName,
                    ContactName = c.ContactName,
                    CustomerEmail = c.Email,
                    Address = c.Address,
                    City = c.City,
                    PostalCode = c.PostalCode,
                    Country = c.Country,
                    Created = c.Created,
                    OrderCount = c.Orders.Count()
                })
                .FirstOrDefaultAsync();

            return customer;
        }

        public async Task<bool> CreateCustomerAsync(CustomerViewModel model)
        {
            try
            {
                var customer = new Customer
                {
                    CustomerName = model.CustomerName,
                    ContactName = model.ContactName,
                    Email = model.CustomerEmail,
                    Address = model.Address,
                    City = model.City,
                    PostalCode = model.PostalCode,
                    Country = model.Country,
                    Created = DateTime.Now
                };

                _dbContext.Customers.Add(customer);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCustomerAsync(CustomerViewModel model)
        {
            try
            {
                var customer = await _dbContext.Customers
                    .FirstOrDefaultAsync(c => c.CustomerId == model.CustomerId);

                if (customer == null)
                    return false;

                customer.CustomerName = model.CustomerName;
                customer.ContactName = model.ContactName;
                customer.Email = model.CustomerEmail;
                customer.Address = model.Address;
                customer.City = model.City;
                customer.PostalCode = model.PostalCode;
                customer.Country = model.Country;

                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            try
            {
                var customer = await _dbContext.Customers.FindAsync(id);
                if (customer == null)
                    return false;

                // Check if customer has orders
                var hasOrders = await HasOrdersAsync(id);
                if (hasOrders)
                    return false;

                _dbContext.Customers.Remove(customer);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> CustomerExistsAsync(int id)
        {
            return await _dbContext.Customers.AnyAsync(c => c.CustomerId == id);
        }

        public async Task<bool> HasOrdersAsync(int id)
        {
            return await _dbContext.Orders.AnyAsync(o => o.CustomerId == id);
        }
    }
}