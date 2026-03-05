using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StoreManagement.Data;
using StoreManagement.Models.Entities;
using StoreManagement.Models.ViewModels;
using StoreManagement.Services.Interfaces;

namespace StoreManagement.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _dbContext;

        public ProductService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<ProductListViewModel>> GetAllProductsAsync()
        {
            return await _dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .Select(p => new ProductListViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    CategoryName = p.Category != null ? p.Category.CategoryName : "N/A",
                    SupplierName = p.Supplier != null ? p.Supplier.SupplierName : "N/A",
                    Stock = p.Stock,
                    StockUnit = p.StockUnit,
                    ReorderLevel = p.ReorderLevel
                })
                .ToListAsync();
        }

        // New: server-side search / filter / sort
        public async Task<IEnumerable<ProductListViewModel>> GetProductsAsync(string? search, int? categoryId, int? supplierId, string? sort)
        {
            var query = _dbContext.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p =>
                    p.ProductName.ToLower().Contains(s) ||
                    (p.Category != null && p.Category.CategoryName.ToLower().Contains(s)) ||
                    (p.Supplier != null && p.Supplier.SupplierName.ToLower().Contains(s))
                );
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (supplierId.HasValue)
            {
                query = query.Where(p => p.SupplierId == supplierId.Value);
            }

            // simple sort keys: name_asc (default), name_desc, price_asc, price_desc, stock_asc, stock_desc
            query = sort switch
            {
                "name_desc" => query.OrderByDescending(p => p.ProductName),
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "stock_asc" => query.OrderBy(p => p.Stock),
                "stock_desc" => query.OrderByDescending(p => p.Stock),
                _ => query.OrderBy(p => p.ProductName)
            };

            return await query.Select(p => new ProductListViewModel
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                Price = p.Price,
                CategoryName = p.Category != null ? p.Category.CategoryName : "N/A",
                SupplierName = p.Supplier != null ? p.Supplier.SupplierName : "N/A",
                Stock = p.Stock,
                StockUnit = p.StockUnit,
                ReorderLevel = p.ReorderLevel
            }).ToListAsync();
        }

        // New: lists for filters
        public async Task<(List<SelectListItem> Categories, List<SelectListItem> Suppliers)> GetFilterListsAsync()
        {
            var categories = await _dbContext.Categories
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
                .ToListAsync();

            var suppliers = await _dbContext.Suppliers
                .AsNoTracking()
                .OrderBy(s => s.SupplierName)
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.SupplierName
                })
                .ToListAsync();

            return (categories, suppliers);
        }

        public async Task<ProductViewModel> GetProductByIdAsync(int id)
        {
            var product = await _dbContext.Products
                .Include(p => p.Category)
                .Include(p => p.Supplier)
                .FirstOrDefaultAsync(p => p.ProductId == id);
            if (product == null) throw new NotFoundException($"Product {id} not found.");
            return new ProductViewModel 
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                CategoryId = product.CategoryId,
                SupplierId = product.SupplierId,
                Unit = product.Unit,
                Price = product.Price,
                CategoryName = product.Category?.CategoryName,
                SupplierName = product.Supplier?.SupplierName
            };
        }

        public async Task<ProductViewModel> GetProductForCreateAsync() 
        {
            var categories = await _dbContext.Categories
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
                .ToListAsync();
            var suppliers = await _dbContext.Suppliers
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.SupplierName
                })
                .ToListAsync();
            return new ProductViewModel
            {
                Categories = categories,
                Suppliers = suppliers
            };

        }

        public async Task<ProductViewModel> GetProductForEditAsync(int id)
        {
            //var product = await GetProductForEditAsync(id);

            // Get product data directly as ViewModel
            var product = await _dbContext.Products
                .AsNoTracking()
                .Where(p => p.ProductId == id)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    SupplierId = p.SupplierId,
                    CategoryId = p.CategoryId,
                    Unit = p.Unit,
                    Price = p.Price,
                    Stock = p.Stock,
                    StockUnit = p.StockUnit,
                    ReorderLevel = p.ReorderLevel
                })
                .FirstOrDefaultAsync();

            if (product == null) return null!;

            var categories = await _dbContext.Categories
                .AsNoTracking()
                .Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName
                })
                .ToListAsync();
            var suppliers = await _dbContext.Suppliers
                .AsNoTracking()
                .Select(s => new SelectListItem
                {
                    Value = s.SupplierId.ToString(),
                    Text = s.SupplierName
                })
                .ToListAsync();

            product.Categories = categories;
            product.Suppliers = suppliers;

            return product;
        }

        public async Task<bool> CreateProductAsync(ProductViewModel model)
        {
            try
            {
                var product = new Product
                {
                    ProductName = model.ProductName,
                    CategoryId = model.CategoryId,
                    SupplierId = model.SupplierId,
                    Unit = model.Unit,
                    Price = model.Price,
                    Stock = model.Stock,
                    StockUnit = model.StockUnit,
                    ReorderLevel = model.ReorderLevel
                };

                // Validate category/supplier exist if provided
                if (model.CategoryId.HasValue)
                {
                    var category = await _dbContext.Categories.FindAsync(model.CategoryId.Value);
                    if (category == null) throw new NotFoundException($"Category {model.CategoryId.Value} not found.");
                }

                if (model.SupplierId.HasValue)
                {
                    var supplier = await _dbContext.Suppliers.FindAsync(model.SupplierId.Value);
                    if (supplier == null) throw new NotFoundException($"Supplier {model.SupplierId.Value} not found.");
                }

                _dbContext.Products.Add(product);
                await _dbContext.SaveChangesAsync();

                if (model.Stock != 0)
                {
                    _dbContext.StockMovements.Add(new StockMovement
                    {
                        ProductId = product.ProductId,
                        Change = model.Stock,
                        Reason = "Initial stock",
                        CreatedBy = null
                    });
                    await _dbContext.SaveChangesAsync();
                }

                return true;
            }
            catch
            {
                return false;
            }
            
        }

        public async Task<bool> UpdateProductAsync(ProductViewModel model)
        {
            try
            {
                var existingProduct = await _dbContext.Products.FindAsync(model.ProductId);
                if (existingProduct == null) throw new NotFoundException($"Product {model.ProductId} not found.");
                
                // record change if stock was edited manually (optional)
                var stockDiff = model.Stock - existingProduct.Stock;

                // update allowed fields
                existingProduct.ProductName = model.ProductName;
                existingProduct.CategoryId = model.CategoryId;
                existingProduct.SupplierId = model.SupplierId;
                existingProduct.Unit = model.Unit;
                existingProduct.Price = model.Price;
                existingProduct.Stock = model.Stock;
                existingProduct.StockUnit = model.StockUnit;
                existingProduct.ReorderLevel = model.ReorderLevel;

                using var tx = await _dbContext.Database.BeginTransactionAsync();
                _dbContext.Products.Update(existingProduct);
                await _dbContext.SaveChangesAsync();

                if (stockDiff != 0)
                {
                    _dbContext.StockMovements.Add(new StockMovement
                    {
                        ProductId = existingProduct.ProductId,
                        Change = stockDiff,
                        Reason = "Manual adjustment",
                        CreatedBy = null
                    });
                    await _dbContext.SaveChangesAsync();
                }

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null) throw new NotFoundException($"Product {id} not found.");
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ProductExistsAsync(int id)
        {
            return await _dbContext.Products.AnyAsync(p => p.ProductId == id);
        }
    }
}