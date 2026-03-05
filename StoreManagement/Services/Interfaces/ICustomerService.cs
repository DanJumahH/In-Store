using StoreManagement.Models.Entities;
using StoreManagement.Models.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StoreManagement.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerViewModel>> GetAllCustomersAsync();
        Task<CustomerViewModel> GetCustomerByIdAsync(int id);
        Task<bool> CreateCustomerAsync(CustomerViewModel model);
        Task<bool> UpdateCustomerAsync(CustomerViewModel model);
        Task<bool> DeleteCustomerAsync(int id);
        Task<bool> CustomerExistsAsync(int id);
        Task<bool> HasOrdersAsync(int id);
    }
}
