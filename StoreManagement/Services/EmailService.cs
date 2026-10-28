using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;
using StoreManagement.Settings;

namespace StoreManagement.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;

        public EmailService(
            EmailSettings emailSettings, 
            ILogger<EmailService> logger, 
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration)
        {
            _emailSettings = emailSettings;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
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

        public async Task<bool> SendReceiptEmailAsync(string toEmail, ReceiptViewModel receipt)
        {
            try
            {
                // Fallback to default email if customer email is empty
                var recipientEmail = string.IsNullOrWhiteSpace(toEmail) 
                    ? _configuration["Paystack:DefaultCustomerEmail"]
                    : toEmail;

                if (string.IsNullOrWhiteSpace(recipientEmail))
                {
                    _logger.LogWarning("No email address available for receipt. Customer email: {Email}, Default email not configured", toEmail);
                    return false;
                }

                // Load the HTML template
                var templatePath = Path.Combine(
                    _webHostEnvironment.ContentRootPath,
                    "Services", "EmailTemplates", "ReceiptTemplate.html"
                );

                if (!File.Exists(templatePath))
                {
                    _logger.LogError("Receipt template not found: {TemplatePath}", templatePath);
                    throw new FileNotFoundException($"Template not found: {templatePath}");
                }

                var htmlTemplate = await File.ReadAllTextAsync(templatePath);

                // Replace placeholders with actual values
                var html = htmlTemplate
                    .Replace("{{StoreName}}", receipt.StoreName ?? "Store")
                    .Replace("{{OrderId}}", receipt.OrderId.ToString())
                    .Replace("{{OrderDate}}", receipt.OrderDateFormatted)
                    .Replace("{{PaymentDate}}", receipt.PaymentDateFormatted)
                    .Replace("{{PaymentReference}}", receipt.PaymentReference ?? "—")
                    .Replace("{{CustomerName}}", receipt.CustomerName ?? "")
                    .Replace("{{CustomerEmail}}", receipt.CustomerEmail ?? recipientEmail)
                    .Replace("{{Subtotal}}", receipt.Subtotal.ToString("F2"))
                    .Replace("{{Charges}}", receipt.Charges.ToString("F2"))
                    .Replace("{{Total}}", receipt.Total.ToString("F2"))
                    .Replace("{{GeneratedDate}}", DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

                // Handle conditional charges section
                if (receipt.Charges > 0)
                {
                    html = html
                        .Replace("{{#ShowCharges}}", "")
                        .Replace("{{/ShowCharges}}", "");
                }
                else
                {
                    // Remove the charges section if no charges
                    html = Regex.Replace(html, @"{{#ShowCharges}}.*?{{/ShowCharges}}", "", RegexOptions.Singleline);
                }

                // Handle items loop
                var itemsHtml = string.Empty;
                if (receipt.Items != null && receipt.Items.Any())
                {
                    foreach (var item in receipt.Items)
                    {
                        itemsHtml += $@"
                <tr>
                    <td>{item.ProductName}</td>
                    <td class=""text-end"">{item.Quantity}</td>
                    <td class=""text-end"">₦{item.UnitPrice:F2}</td>
                    <td class=""text-end"">₦{item.Total:F2}</td>
                </tr>";
                    }
                }

                // Replace items placeholder with actual items
                html = Regex.Replace(
                    html,
                    @"{{#Items}}.*?{{/Items}}",
                    itemsHtml,
                    RegexOptions.Singleline
                );

                // Send the email
                var emailSent = await SendEmailAsync(recipientEmail, $"Receipt - Order #{receipt.OrderId}", html);

                if (emailSent)
                {
                    _logger.LogInformation("Receipt email sent to {Email} for order {OrderId}", recipientEmail, receipt.OrderId);
                }

                return emailSent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending receipt email for order {OrderId}", receipt.OrderId);
                return false;
            }
        }
    }
}