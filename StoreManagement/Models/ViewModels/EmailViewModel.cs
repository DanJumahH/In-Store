namespace StoreManagement.Models.ViewModels
{
    public class EmailViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public int StaffId { get; set; }
        public string AppUrl { get; set; } = string.Empty;
    }
}