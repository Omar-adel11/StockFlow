using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions.NotFound;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class SupplierService(IAppDbContext _context, ILogger<SupplierService> _logger) : ISupplierService
    {
        private DbSet<Supplier> SupplierSet => _context.Suppliers;

        public async Task<SuppliersDtos.SupplierResponse> CreateSupplierAsync(SuppliersDtos.SupplierCreateRequest createRequest,int businessId)
        {
            var supplier = new Supplier
            {
                Name = createRequest.Name,
                ContactEmail = createRequest.ContactEmail,
                ContactPhone = createRequest.ContactPhone,
                Address = createRequest.Address,
                IsActive = true,
                BusinessId = businessId
            };

            await SupplierSet.AddAsync(supplier);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Supplier created. SupplierId: {SupplierId}", supplier.Id);

            return MapToResponse(supplier);
        }

        public async Task<bool> DeleteSupplierAsync(int id, int businessId)
        {
            var supplier = await GetSupplierEntityAsync(id);
            

            await ValidateNoAssociatedActiveProductsAsync(supplier.Id, supplier.Name);

            _context.Suppliers.Remove(supplier);
            var result = await _context.SaveChangesAsync();
            if (result > 0) _logger.LogInformation("Supplier deleted. SupplierId: {SupplierId}", supplier.Id);
            return result > 0;
        }

        public async Task<IReadOnlyCollection<SuppliersDtos.SupplierResponse>> GetAllSuppliersAsync(string? search, int businessId)
        {
            var query = SupplierSet.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(c => c.Name.Contains(search) || c.ContactEmail.Contains(search));
            }
            var suppliers = await query.ToListAsync();
            _logger.LogDebug("Retrieved {ResultCount} suppliers. SearchApplied: {SearchApplied}", suppliers.Count, !string.IsNullOrWhiteSpace(search));
            return suppliers.Select(MapToResponse).ToList();
        }

        public async Task<SuppliersDtos.SupplierResponse?> GetSupplierAsync(int id)
        {
            var supplier = await GetSupplierEntityAsync(id);
            _logger.LogDebug("Supplier retrieved. SupplierId: {SupplierId}", supplier.Id);
            return MapToResponse(supplier);
        }

        public async Task<SuppliersDtos.SupplierResponse?> UpdateSupplierAsync(int id, SuppliersDtos.SupplierUpdateRequest updateRequest, int businessId)
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
            _logger.LogInformation("Supplier updated. SupplierId: {SupplierId}", supplier.Id);

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