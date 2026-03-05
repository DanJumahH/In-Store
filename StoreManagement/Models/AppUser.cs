using Microsoft.AspNetCore.Identity;
using StoreManagement.Models.Entities;

namespace StoreManagement.Models
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public int? StaffId { get; set; }
        public Staff? Staff { get; set; }

    }
}
