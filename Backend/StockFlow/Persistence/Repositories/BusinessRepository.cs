using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public class BusinessRepository : IBusinessRepository
    {
        private readonly AppDbContext _dbContext;

        public BusinessRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Business?> GetByIdAsync(int id)
        {
            // Ignore global tenant query filters so SaaS Admin can fetch any business across tenants
            return await _dbContext.Business
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public void Update(Business business)
        {
            _dbContext.Business.Update(business);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
