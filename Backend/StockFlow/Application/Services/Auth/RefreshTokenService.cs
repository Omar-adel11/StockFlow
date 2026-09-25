using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Interfaces.AuthInterfaces;


namespace Application.Services.Auth
{
    public class RefreshTokenService(ICacheService cacheService) : IRefreshTokenService
    {
        private static string BuildKey(string token) => $"refresh_token:{token}";

        public async Task<string> GenerateAndStoreAsync(int userId, TimeSpan lifetime)
        {
            var token = GenerateSecureToken();
            await cacheService.SetCacheValueAsync(BuildKey(token), userId, lifetime);
            return token;
        }

        public async Task<int?> ValidateAndGetUserIdAsync(string refreshToken)
        {
            var json = await cacheService.GetAsync(BuildKey(refreshToken));
            if (json is null)
            {
                return null; // missing = expired (TTL passed) or never existed or already revoked
            }

            return JsonSerializer.Deserialize<int>(json);
        }

        public async Task RevokeAsync(string refreshToken)
        {
            await cacheService.RemoveAsync(BuildKey(refreshToken));
        }

      

        private static string GenerateSecureToken()
        {
            // Cryptographically secure random bytes - NOT Guid.NewGuid(),
            // which is not designed to resist prediction of security tokens.
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", ""); // URL-safe, in case it ever ends up in a query string
        }
    }
}
