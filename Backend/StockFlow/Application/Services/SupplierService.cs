using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions.NotFound;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class SupplierService(IAppDbContext _context) : ISupplierService
    {
        private DbSet<Supplier> SupplierSet => _context.Suppliers;

        public async Task<SuppliersDtos.SupplierResponse> CreateSupplierAsync(SuppliersDtos.SupplierCreateRequest createRequest)
        {
            var supplier = new Supplier
            {
                Name = createRequest.Name,
                ContactEmail = createRequest.ContactEmail,
                ContactPhone = createRequest.ContactPhone,
                Address = createRequest.Address,
                IsActive = true
            };

            await SupplierSet.AddAsync(supplier);
            await _context.SaveChangesAsync();

            return MapToResponse(supplier);
        }

        public async Task<bool> DeleteSupplierAsync(int id)
        {
            var supplier = await GetSupplierEntityAsync(id);
            if (!supplier.IsActive)
            {
                return true; // Already deactivated
            }

            await ValidateNoAssociatedActiveProductsAsync(supplier.Id, supplier.Name);

            supplier.IsActive = false;
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IReadOnlyCollection<SuppliersDtos.SupplierResponse>> GetAllSuppliersAsync()
        {
            // Execute the query first in-memory so MapToResponse works safely
            var suppliers = await SupplierSet
                .AsNoTracking()
                .ToListAsync();

            return suppliers
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<SuppliersDtos.SupplierResponse?> GetSupplierAsync(int id)
        {
            var supplier = await GetSupplierEntityAsync(id);
            return MapToResponse(supplier);
        }

        public async Task<SuppliersDtos.SupplierResponse?> UpdateSupplierAsync(int id, SuppliersDtos.SupplierUpdateRequest updateRequest)
        {
            var supplier = await GetSupplierEntityAsync(id);

            // Validate active product dependencies before deactivating
            if (!updateRequest.IsActive && supplier.IsActive)
            {
                await ValidateNoAssociatedActiveProductsAsync(supplier.Id, supplier.Name);
            }

            supplier.Name = updateRequest.Name;
            supplier.ContactEmail = updateRequest.ContactEmail;
            supplier.ContactPhone = updateRequest.ContactPhone;
            supplier.Address = updateRequest.Address;
            supplier.IsActive = updateRequest.IsActive;

            await _context.SaveChangesAsync();

            return MapToResponse(supplier);
        }

        #region Helper Methods

        private async Task ValidateNoAssociatedActiveProductsAsync(int supplierId, string supplierName)
        {
            var hasActiveProducts = await _context.Products
                .AnyAsync(p => p.PreferredSupplierId == supplierId && p.IsActive);

            if (hasActiveProducts)
            {
                throw new InvalidOperationException($"Cannot deactivate or delete supplier '{supplierName}' because active products are assigned to it.");
            }
        }

        private async Task<Supplier> GetSupplierEntityAsync(int id)
        {
            var supplier = await SupplierSet.FirstOrDefaultAsync(s => s.Id == id);
            if (supplier == null)
            {
                throw new SupplierNotFoundException();
            }

            return supplier;
        }

        private static SuppliersDtos.SupplierResponse MapToResponse(Supplier supplier)
        {
            return new SuppliersDtos.SupplierResponse(
                supplier.Id,
                supplier.Name,
                supplier.ContactEmail,
                supplier.ContactPhone,
                supplier.Address,
                supplier.IsActive
            );
        }

        #endregion
    }
}