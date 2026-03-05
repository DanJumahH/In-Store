using StoreManagement.Models.Entities;
using StoreManagement.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StoreManagement.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductListViewModel>> GetAllProductsAsync();

        // Advanced filtering and sorting
        Task<IEnumerable<ProductListViewModel>> GetProductsAsync(string? search, int? categoryId, int? supplierId, string? sort);
        Task<(List<SelectListItem> Categories, List<SelectListItem> Suppliers)> GetFilterListsAsync();

        Task<ProductViewModel> GetProductByIdAsync(int id);
        Task<ProductViewModel> GetProductForCreateAsync();
        Task<ProductViewModel> GetProductForEditAsync(int id);
        Task<bool> CreateProductAsync(ProductViewModel model);
        Task<bool> UpdateProductAsync(ProductViewModel model);
        Task<bool> DeleteProductAsync(int id);
        Task<bool> ProductExistsAsync(int id);
        
        
    }
}