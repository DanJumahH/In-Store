using StoreManagement.Models.ViewModels;

namespace StoreManagement.Services.Interfaces
{
    public interface IPaystackService
    {
        /// <summary>
        /// Initialize a payment transaction with Paystack
        /// </summary>
        Task<PaystackInitializeResponse> InitializePaymentAsync(string email, decimal amountInNaira, string reference, string callbackUrl);

        /// <summary>
        /// Verify a payment transaction with Paystack
        /// </summary>
        Task<PaystackVerifyResponse> VerifyPaymentAsync(string reference);

        /// <summary>
        /// Validate webhook signature from Paystack
        /// </summary>
        bool ValidateWebhookSignature(string body, string signature);
    }
}