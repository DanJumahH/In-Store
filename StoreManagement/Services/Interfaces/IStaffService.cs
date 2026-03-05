using StoreManagement.Models.ViewModels;

namespace StoreManagement.Services.Interfaces
{
    public interface IStaffService
    {
        Task<IEnumerable<StaffViewModel>> GetAllStaffAsync();
        Task<StaffViewModel?> GetStaffByIdAsync(int id);
        Task<StaffViewModel> GetStaffForCreateAsync();
        Task<StaffViewModel> GetStaffForEditAsync(int id);
        Task<StaffCreateResultViewModel> CreateStaffAsync(StaffViewModel model);
        Task<bool> UpdateStaffAsync(StaffViewModel model);
        Task<bool> DeleteStaffAsync(int id);
        Task<bool> StaffExistsAsync(int id);
        Task<bool> HasOrdersAsync(int id);
        Task<bool> IsAppUserRole(string role);
    }
}