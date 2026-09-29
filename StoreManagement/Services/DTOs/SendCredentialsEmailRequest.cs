namespace StoreManagement.Services.DTOs
{
    public class SendCredentialsEmailRequest
    {
        public int StaffId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set;}
    }
}
