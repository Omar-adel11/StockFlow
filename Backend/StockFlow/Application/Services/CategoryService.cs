using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions.NotFound;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class CategoryService(IAppDbContext _context) : ICategoryService
    {
        private DbSet<Category> CategorySet => _context.Categories;
        public async Task<CategoryDtos.Response> CreateCategoryAsync(CategoryDtos.CreateRequest createRequest)
        {
            var category = new Category
            {
                Name = createRequest.Name,
                Description = createRequest.Description
            };
            await CategorySet.AddAsync(category);
            await _context.SaveChangesAsync();
            return new CategoryDtos.Response(category.Id, category.Name, category.Description);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await GetCategory(id);
            var hasActiveProducts = await _context.Products
                .AnyAsync(p => p.CategoryId == id && p.IsActive);

            if (hasActiveProducts)
            {
                throw new InvalidOperationException($"Cannot deactivate or delete category '{category.Name}' because active products belong to it.");
            }
            CategorySet.Remove(category);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IReadOnlyCollection<CategoryDtos.Response>> GetAllCategoriesAsync()
        {
            return await CategorySet.AsNoTracking().Select(c => new CategoryDtos.Response(c.Id,c.Name,c.Description)).ToListAsync();
        }

        public async Task<CategoryDtos.Response?> GetCategoryAsync(int id)
        {
            var category = await GetCategory(id);
            var response = new CategoryDtos.Response(category.Id, category.Name, category.Description);
            return response;
        }

        public async Task<CategoryDtos.Response?> UpdateCategoryAsync(int Id, CategoryDtos.UpdateRequest updateRequest)
        {
            var category = await GetCategory(Id);

            category.Name = updateRequest.Name;
            category.Description = updateRequest.Description;

            await _context.SaveChangesAsync();
            return new CategoryDtos.Response(category.Id,category.Name,category.Description);
        }

        private  async Task<Category> GetCategory(int id)
        {
            var category = await CategorySet.FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
            {
                throw new CategoryNotFoundException();
            }
            return category;
        }
    }
}
