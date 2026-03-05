using StoreManagement.Models.ViewModels;

namespace StoreManagement.Services.Interfaces
{
    public interface IShipperService
    {
        Task<IEnumerable<ShipperViewModel>> GetAllShippersAsync();
        Task<ShipperViewModel?> GetShipperByIdAsync(int id);
        Task<bool> CreateShipperAsync(ShipperViewModel model);
        Task<bool> UpdateShipperAsync(ShipperViewModel model);
        Task<bool> DeleteShipperAsync(int id);
        Task<bool> ShipperExistsAsync(int id);
        Task<bool> HasOrdersAsync(int id);
    }
}