using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.AuthInterfaces
{
    public interface IRefreshTokenService
    {
        Task<string> GenerateAndStoreAsync(int userId, TimeSpan lifetime);


        Task<int?> ValidateAndGetUserIdAsync(string refreshToken);


        Task RevokeAsync(string refreshToken);
    }
}
