using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace StoreManagement.Models.ViewModels
{
    public class StaffViewModel
    {
        public int StaffId { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(20, ErrorMessage = "First name cannot exceed 20 characters")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(20, ErrorMessage = "Last name cannot exceed 20 characters")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        public DateOnly? BirthDate { get; set; }

        [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Staff role is required")]
        [Display(Name = "Staff Role")]
        public string StaffRole { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // For display
        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Has App Access")]
        public bool IsAppUser { get; set; }

        [Display(Name = "Number of Orders")]
        public int OrderCount { get; set; }

        // For dropdowns
        public List<SelectListItem>? AvailableRoles { get; set; }
    }

    public class StaffCreateResultViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StaffId { get; set; }
        public bool AppUserCreated { get; set; }
        public string? GeneratedPassword { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
    }
}