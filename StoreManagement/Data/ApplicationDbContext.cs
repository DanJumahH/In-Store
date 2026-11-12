using StoreManagement.Models;
using StoreManagement.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace StoreManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            this.ChangeTracker.LazyLoadingEnabled = false;
            this.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Shipper> Shippers { get; set; }
        public virtual DbSet<Staff> Staffs { get; set; }
        public virtual DbSet<Supplier> Suppliers { get; set; }
        public virtual DbSet<StockMovement> StockMovements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.Property(e => e.CategoryId).ValueGeneratedOnAdd().HasColumnName("CategoryID");
                entity.Property(e => e.CategoryName).HasMaxLength(255).IsUnicode(false);
                entity.Property(e => e.Description).HasMaxLength(255).IsUnicode(false);
            });

            modelBuilder.Entity<Customer>(entity =>
            {
                entity.HasKey(e => e.CustomerId).HasName("PK__Customer__A4AE64B8F8F012AF");
                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
                entity.Property(e => e.Address).HasMaxLength(255).IsUnicode(false);
                entity.Property(e => e.City).HasMaxLength(255).IsUnicode(false);
                entity.Property(e => e.ContactName).HasMaxLength(255).IsUnicode(false);
                entity.Property(e => e.Country).HasMaxLength(255).IsUnicode(false);
                entity.Property(e => e.Created).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.CustomerName).HasMaxLength(255).IsUnicode(false);
                entity.Property(e => e.PostalCode).HasMaxLength(20).IsUnicode(false);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.OrderId).HasName("PK__Orders__C3905BAFC198D7B0");
                entity.Property(e => e.OrderId).ValueGeneratedNever().HasColumnName("OrderID");
                entity.Property(e => e.CustomerId).HasColumnName("CustomerID");
                entity.Property(e => e.EmployeeId).HasColumnName("EmployeeID");
                entity.Property(e => e.OrderDate).HasColumnType("datetime");
                entity.Property(e => e.ShipperId).HasColumnName("ShipperID");

                entity.Property(e => e.PaymentReference).HasMaxLength(100);
                entity.Property(e => e.PaymentMethod).HasConversion<int>();
                entity.Property(e => e.PaymentStatus).HasConversion<int>();
                entity.Property(e => e.PaymentDate).HasColumnType("datetime2");
                entity.Property(e => e.PaymentNotes).HasMaxLength(500);
            });

            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.OrderDetailId).HasName("PK__OrderDet__D3B9D30C23C9571B");
                entity.Property(e => e.OrderDetailId).HasColumnName("OrderDetailID");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").HasColumnType("datetime");
                entity.Property(e => e.OrderId).HasColumnName("OrderID");
                entity.Property(e => e.ProductId).HasColumnName("ProductID");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductId).HasName("PK__Products__B40CC6ED56BF9B6F");
                entity.Property(e => e.ProductId).HasColumnName("ProductID");
                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
                entity.Property(e => e.Price).HasColumnType("money");
                entity.Property(e => e.ProductName).HasMaxLength(255).IsUnicode(false);
                entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
                entity.Property(e => e.Unit).HasMaxLength(255).IsUnicode(false);
                entity.Property(e => e.Stock).HasDefaultValue(0).HasColumnName("Stock");
                entity.Property(e => e.StockUnit).HasMaxLength(50).IsUnicode(false).HasColumnName("StockUnit");
                entity.Property(e => e.ReorderLevel).HasDefaultValue(0).HasColumnName("ReorderLevel");

                entity.Property(e => e.Currency).HasMaxLength(3).HasDefaultValue("NGN");
            });

            modelBuilder.Entity<Shipper>(entity =>
            {
                entity.HasKey(e => e.ShipperId).HasName("PK__Shippers__1F8AFE5920FE7363");
                entity.Property(e => e.Phone).HasMaxLength(15);
                entity.Property(e => e.ShipperName).HasMaxLength(255).IsUnicode(false);
            });

            modelBuilder.Entity<Staff>(entity =>
            {
                entity.HasKey(e => e.StaffId).HasName("PK__Staffs__96D4AAF7CF7FF57C");
                entity.Property(e => e.StaffId).HasColumnName("StaffID");
                entity.Property(e => e.FirstName).HasMaxLength(20).IsUnicode(false);
                entity.Property(e => e.LastName).HasMaxLength(20).IsUnicode(false);
                entity.Property(e => e.Email).HasMaxLength(100).IsUnicode(false);
                entity.Property(e => e.StaffRole).HasMaxLength(50).IsUnicode(false);
            });

            modelBuilder.Entity<Supplier>(entity =>
            {
                entity.HasKey(e => e.SupplierId).HasName("PK__Supplier__4BE66694BE9EDE1A");
                entity.Property(e => e.SupplierId).HasColumnName("SupplierID");
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(20);
                entity.Property(e => e.ContactName).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(20);
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.PostalCode).HasMaxLength(20);
                entity.Property(e => e.SupplierName).HasMaxLength(100);
            });

            modelBuilder.Entity<StockMovement>(entity =>
            {
                entity.HasKey(e => e.StockMovementId);
                entity.Property(e => e.Change).IsRequired();
                entity.Property(e => e.Reason).HasMaxLength(200);
                entity.Property(e => e.CreatedBy).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            });

            // Define relationships
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(m => m.Orders)
                .HasForeignKey(f => f.CustomerId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Staff)
                .WithMany(m => m.Orders)
                .HasForeignKey(f => f.EmployeeId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Shipper)
                .WithMany(m => m.Orders)
                .HasForeignKey(f => f.ShipperId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(m => m.OrderDetails)
                .HasForeignKey(f => f.OrderId);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(m => m.OrderDetails)
                .HasForeignKey(f => f.ProductId);

            modelBuilder.Entity<Product>()
                .HasOne(o => o.Category)
                .WithMany(m => m.Products)
                .HasForeignKey(f => f.CategoryId);

            modelBuilder.Entity<Product>()
                .HasOne(o => o.Supplier)
                .WithMany(m => m.Products)
                .HasForeignKey(f => f.SupplierId);

            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<StockMovement>()
                .HasOne(o => o.Product)
                .WithMany(m => m.StockMovements)
                .HasForeignKey(sm => sm.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AppUser>()
                .HasOne(o => o.Staff)
                .WithMany()
                .HasForeignKey(f => f.StaffId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}