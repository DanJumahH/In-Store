namespace StoreManagement.Services.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendStaffCredentialsAsync(Models.ViewModels.EmailViewModel model);
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody);
    }
}
