using Microsoft.AspNetCore.Mvc.Rendering;
using StoreManagement.Models.Enums;

namespace StoreManagement.Models.ViewModels
{
    public class OrderFilterViewModel
    {
        public string? SearchTerm { get; set; }
        public OrderStatus? Status { get; set; }
        public int? StaffId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // For dropdowns
        public List<SelectListItem>? StatusOptions { get; set; }
        public List<SelectListItem>? StaffOptions { get; set; }

        // Results
        public IEnumerable<OrderListViewModel> Orders { get; set; } = new List<OrderListViewModel>();

        public bool HasActiveFilters()
        {
            return !string.IsNullOrWhiteSpace(SearchTerm)
                   || Status.HasValue
                   || StaffId.HasValue
                   || StartDate.HasValue
                   || EndDate.HasValue;
        }
    }
}