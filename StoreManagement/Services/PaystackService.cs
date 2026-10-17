using Microsoft.Extensions.Options;
using StoreManagement.Models.Entities;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace StoreManagement.Services
{
    public class PaystackService : IPaystackService
    {
        private readonly HttpClient _httpClient;
        private readonly PaystackSettings _settings;
        private readonly ILogger<PaystackService> _logger;

        public PaystackService(HttpClient httpClient, IOptions<PaystackSettings> settings, ILogger<PaystackService> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;

            // Set default headers
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.SecretKey}");
        }

        public async Task<PaystackInitializeResponse> InitializePaymentAsync(string email, decimal amountInNaira, string reference, string callbackUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(_settings.SecretKey))
                {
                    _logger.LogError("Paystack SecretKey is not configured.");
                    return new PaystackInitializeResponse { Status = false, Message = "Payment service not configured." };
                }

                // Amount in Kobo (multiply by 100)
                int amountInKobo = (int)(amountInNaira * 100);

                var payload = new
                {
                    email,
                    amount = amountInKobo,
                    reference,
                    callback_url = callbackUrl,
                    metadata = new
                    {
                        reference,
                        timestamp = DateTime.UtcNow
                    }
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync("https://api.paystack.co/transaction/initialize", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Paystack initialization failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return new PaystackInitializeResponse { Status = false, Message = "Failed to initialize payment." };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                {
                    var root = doc.RootElement;

                    if (root.TryGetProperty("data", out var data))
                    {
                        var result = new PaystackInitializeResponse
                        {
                            Status = root.GetProperty("status").GetBoolean(),
                            Message = root.GetProperty("message").GetString() ?? string.Empty,
                            AuthorizationUrl = data.GetProperty("authorization_url").GetString() ?? string.Empty,
                            Reference = data.GetProperty("reference").GetString() ?? string.Empty,
                            AccessCode = data.GetProperty("access_code").GetString() ?? string.Empty
                        };

                        _logger.LogInformation("Payment initialized successfully for reference: {Reference}", reference);
                        return result;
                    }
                }

                return new PaystackInitializeResponse { Status = false, Message = "Invalid response from Paystack." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Paystack payment for reference: {Reference}", reference);
                return new PaystackInitializeResponse { Status = false, Message = "An error occurred while initializing payment." };
            }
        }

        public async Task<PaystackVerifyResponse> VerifyPaymentAsync(string reference)
        {
            try
            {
                if (string.IsNullOrEmpty(_settings.SecretKey))
                {
                    _logger.LogError("Paystack SecretKey is not configured.");
                    return new PaystackVerifyResponse { Status = false, Message = "Payment service not configured." };
                }

                var response = await _httpClient.GetAsync($"https://api.paystack.co/transaction/verify/{reference}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Paystack verification failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return new PaystackVerifyResponse { Status = false, Message = "Failed to verify payment." };
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                {
                    var root = doc.RootElement;

                    if (root.TryGetProperty("data", out var data))
                    {
                        var paymentData = new
                        {
                            Status = data.GetProperty("status").GetString() ?? string.Empty,
                            Amount = data.GetProperty("amount").GetInt32(),
                            Currency = data.GetProperty("currency").GetString() ?? string.Empty,
                            Reference = data.GetProperty("reference").GetString() ?? string.Empty,
                            Customer = data.GetProperty("customer").GetProperty("email").GetString() ?? string.Empty
                        };

                        var result = new PaystackVerifyResponse
                        {
                            Status = root.GetProperty("status").GetBoolean(),
                            Message = root.GetProperty("message").GetString() ?? string.Empty,
                            PaymentStatus = paymentData.Status,
                            Amount = paymentData.Amount,
                            Currency = paymentData.Currency,
                            Reference = paymentData.Reference,
                            CustomerEmail = paymentData.Customer
                        };

                        _logger.LogInformation("Payment verified for reference: {Reference}, status: {PaymentStatus}", reference, paymentData.Status);
                        return result;
                    }
                }

                return new PaystackVerifyResponse { Status = false, Message = "Invalid response from Paystack." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying Paystack payment for reference: {Reference}", reference);
                return new PaystackVerifyResponse { Status = false, Message = "An error occurred while verifying payment." };
            }
        }

        public bool ValidateWebhookSignature(string body, string signature)
        {
            try
            {
                if (string.IsNullOrEmpty(_settings.SecretKey))
                {
                    _logger.LogError("Paystack SecretKey is not configured for webhook validation.");
                    return false;
                }

                using (var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_settings.SecretKey)))
                {
                    var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(body));
                    var computedSignature = Convert.ToHexString(hash).ToLower();

                    bool isValid = computedSignature.Equals(signature, StringComparison.OrdinalIgnoreCase);

                    if (!isValid)
                    {
                        _logger.LogWarning("Webhook signature validation failed. Expected: {Expected}, Got: {Got}", computedSignature, signature);
                    }

                    return isValid;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating webhook signature");
                return false;
            }
        }
    }
}