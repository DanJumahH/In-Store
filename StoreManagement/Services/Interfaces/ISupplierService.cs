using StoreManagement.Models.ViewModels;

namespace StoreManagement.Services.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierViewModel>> GetAllSuppliersAsync();
        Task<SupplierViewModel?> GetSupplierByIdAsync(int id);
        Task<bool> CreateSupplierAsync(SupplierViewModel model);
        Task<bool> UpdateSupplierAsync(SupplierViewModel model);
        Task<bool> DeleteSupplierAsync(int id);
        Task<bool> SupplierExistsAsync(int id);
        Task<bool> HasProductsAsync(int id);
    }
}
