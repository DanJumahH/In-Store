using StoreManagement.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace StoreManagement.Controllers
{
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Select(p => new
            {
                p.ProductName,
                p.Price,
                Category = p.Category.CategoryName,
                Supplier = p.Supplier.SupplierName
            })
            .Take(5)
            .ToList();

            var orders = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Select(o => new
                {
                    o.OrderId,
                    o.OrderDate,
                    Customer = o.Customer.CustomerName,
                    Staff = o.Staff != null ? o.Staff.FirstName + " " + o.Staff.LastName : "N/A"
                })
                .Take(5)
                .ToList();

            return Ok(new { products, orders });
        }
    }
}
