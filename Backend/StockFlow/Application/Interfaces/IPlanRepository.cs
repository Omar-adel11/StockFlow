using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces
{
    public interface IPlanRepository
    {
        Task<List<Plan>> GetAllAsync();

        Task<Plan?> GetByIdAsync(int id);

        Task AddAsync(Plan plan);

        void Update(Plan plan);

        void Delete(Plan plan);

        Task SaveChangesAsync();

    }
}
