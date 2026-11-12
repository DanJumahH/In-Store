using StoreManagement.Models.ViewModels;

namespace StoreManagement.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendStaffCredentialsAsync(Models.ViewModels.EmailViewModel model);
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task<bool> SendReceiptEmailAsync(string toEmail, ReceiptViewModel receipt);
    }
}
