using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions.NotFound;
using Microsoft.EntityFrameworkCore;
using static Application.DTOs.ProductDtos;

namespace Application.Services
{
    public class ProductService(IAppDbContext _context) : IProductService
    {
        private  DbSet<Product> _productSet => _context.Products;
        public async Task<ProductResponse> CreateProductAsync(ProductCreateRequest createRequest)
        {
            var product = new Product()
            {
                Name = createRequest.Name,
                SKU = createRequest.ItemSKU,
                CategoryId = createRequest.CategoryId,
                PreferredSupplierId = createRequest?.PreferredSupplierId,
                UnitPrice = createRequest.UnitSellingPrice,
                ReorderLevel = createRequest.ReorderLevel,
                IsActive = true
            };
            await _productSet.AddAsync(product);
            await _context.SaveChangesAsync();
            var p = await GetProductEntityAsync(product.Id);
            return MapToResponse(p);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await GetProductEntityAsync(id);
            if(!product.IsActive)
            {
                return true;
            }
            if(product.InventoryItems.Any(i=>i.QuantityOnHand > 0))
            {
                throw new InvalidOperationException($"Cannot delete Product '{product.Name}' because it currently holds active stock.");
            }
            product.IsActive = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IReadOnlyCollection<ProductResponse>> GetAllProductsAsync()
        {
            var products = await _productSet.AsNoTracking()
                                            .Include(p => p.Category)
                                            .Include(p => p.PreferredSupplier)
                                            .ToListAsync(); 

            return products.Select(MapToResponse).ToList();
        }

        public async Task<ProductResponse?> GetProductByIdAsync(int id)
        {
            var product = await GetProductEntityAsync(id);
            if(product == null)
            {
                throw new ProductNotFoundException();
            }
            return MapToResponse(product);
        }

        public async Task<ProductResponse?> GetProductBySkuAsync(string sku)
        {
            var product = await _productSet.Include(p=>p.Category).Include(p=>p.PreferredSupplier).FirstOrDefaultAsync(p => p.SKU == sku);
            if (product == null)
            {
                throw new ProductNotFoundException();
            }
            return MapToResponse(product);
        }

        public async Task<ProductResponse?> UpdateProductAsync(int id, ProductUpdateRequest updateRequest)
        {
            var product = await GetProductEntityAsync(id);
            product.Name = updateRequest.Name;
            product.UnitPrice = updateRequest.UnitSellingPrice;
            product.ReorderLevel = updateRequest.ReorderLevel;
            product.CategoryId = updateRequest.CategoryId;
            product.PreferredSupplierId = updateRequest.PreferredSupplierId;
            // Handle Soft Delete / Deactivation safely
            if (!updateRequest.IsActive && product.IsActive)
            {
                if(product.InventoryItems.Any(i => i.QuantityOnHand > 0))
                {
                    throw new InvalidOperationException($"Cannot delete Product '{product.Name}' because it currently holds active stock.");
                }

                product.IsActive = updateRequest.IsActive;
            }
            await _context.SaveChangesAsync();
            return MapToResponse(product);
        }
   
        private async Task<Product> GetProductEntityAsync(int id)
        {
            var product =  await _productSet.Include(p => p.PreferredSupplier).Include(p=>p.InventoryItems).Include(p=>p.Category).FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                throw new ProductNotFoundException();
            }
            return product;
        }

        private  ProductResponse MapToResponse(Product product)
        {
            return new ProductResponse
             (
             product.Id,
             product.SKU,
             product.Name,
             product.UnitPrice,
             product.ReorderLevel,
             product.CategoryId,
             product.Category?.Name ?? string.Empty,
             product.PreferredSupplierId,
             product.PreferredSupplier?.Name ?? string.Empty,
             product.IsActive
             );
        }
    }
}
