using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.AuthInterfaces
{
    public interface ICacheRepository
    {
        Task<string?> GetAsync(string Key);
        Task SetAsync(string Key, object Value, TimeSpan? duration);
        Task RemoveAsync(string key);
    }
}
