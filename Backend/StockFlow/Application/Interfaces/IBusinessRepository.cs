using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IBusinessRepository
    {
        Task<Business?> GetByIdAsync(int id);
        void Update(Business business);
        Task SaveChangesAsync();
    }
}
