using System.Net;
using System.Net.Mail;
using StoreManagement.Models.Entities;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;

namespace StoreManagement.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailService(EmailSettings emailSettings, ILogger<EmailService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _emailSettings = emailSettings;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<bool> SendStaffCredentialsAsync(EmailViewModel model)
        {
            try
            {
                var loginUrl = $"{model.AppUrl}/Identity/Account/Login";
                var htmlBody = await LoadAndRenderTemplate("StaffCredentialsTemplate.html", new Dictionary<string, string>
                {
                    { "{FirstName}", model.FirstName },
                    { "{LastName}", model.LastName },
                    { "{Email}", model.Email },
                    { "{Password}", model.Password },
                    { "{Role}", model.Role },
                    { "{LoginUrl}", loginUrl }
                });

                return await SendEmailAsync(model.Email, "Your In-Store Account Credentials", htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending staff credentials email to {Email}", model.Email);
                return false;
            }
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                if (string.IsNullOrEmpty(_emailSettings.SenderEmail) || string.IsNullOrEmpty(_emailSettings.Password))
                {
                    _logger.LogWarning("Email settings not configured properly.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(_emailSettings.SenderEmail))
                {
                    _logger.LogError("SenderEmail is empty or whitespace");
                    return false;
                }

                _logger.LogInformation("SenderEmail value: '{SenderEmail}'", _emailSettings.SenderEmail);
                _logger.LogInformation("SenderName value: '{SenderName}'", _emailSettings.SenderName);

                using (var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.Port))
                {
                    client.EnableSsl = _emailSettings.EnableSsl;
                    client.Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                        Subject = subject,
                        Body = htmlBody,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(toEmail);

                    await client.SendMailAsync(mailMessage);
                    _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {ToEmail}", toEmail);
                return false;
            }
        }

        private async Task<string> LoadAndRenderTemplate(string templateName, Dictionary<string, string> replacements)
        {
            try
            {
                var templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, "Services", "EmailTemplates", templateName);

                if (!File.Exists(templatePath))
                {
                    _logger.LogError("Email template not found at path: {TemplatePath}", templatePath);
                    throw new FileNotFoundException($"Email template '{templateName}' not found.");
                }

                var template = await File.ReadAllTextAsync(templatePath);

                // Replace placeholders
                foreach (var replacement in replacements)
                {
                    template = template.Replace(replacement.Key, replacement.Value);
                }

                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading email template: {TemplateName}", templateName);
                throw;
            }
        }
    }
}