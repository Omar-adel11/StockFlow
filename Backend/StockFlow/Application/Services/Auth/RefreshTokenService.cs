using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Interfaces.AuthInterfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services.Auth
{
    public class RefreshTokenService(
        ICacheService cacheService,
        IAppDbContext context) : IRefreshTokenService
    {
        private static string BuildKey(string token) => $"refresh_token:{token}";

        public async Task<string> GenerateAndStoreAsync(int userId, int businessId, TimeSpan lifetime)
        {
            var token = GenerateSecureToken();
            var expiresAt = DateTime.UtcNow.Add(lifetime);

            // 1. Primary Store: Save in SQL Server Database
            var refreshTokenEntity = new RefreshToken
            {
                Token = token,
                UserId = userId,
                BusinessId = businessId,
                ExpiresAt = expiresAt,
                IsRevoked = false
            };

            await context.RefreshTokens.AddAsync(refreshTokenEntity);
            await context.SaveChangesAsync();

            // 2. Secondary Fast Store: Save in Redis Cache (Safe against Redis down)
            await cacheService.SetCacheValueAsync(BuildKey(token), userId, lifetime);

            return token;
        }

        public async Task<int?> ValidateAndGetUserIdAsync(string refreshToken)
        {
            
            var json = await cacheService.GetAsync(BuildKey(refreshToken));
            if (json is not null)
            {
                try
                {
                    return JsonSerializer.Deserialize<int>(json);
                }
                catch
                {
                    // Corrupted Redis data -> proceed to DB fallback
                }
            }

            
            var tokenEntity = await context.RefreshTokens
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            // Validate token exists, isn't revoked, and hasn't expired
            if (tokenEntity is null || !tokenEntity.IsActive)
            {
                return null;
            }

            // Re-populate Redis cache if it was missing/down previously
            var remainingLifetime = tokenEntity.ExpiresAt - DateTime.UtcNow;
            if (remainingLifetime > TimeSpan.Zero)
            {
                await cacheService.SetCacheValueAsync(BuildKey(refreshToken), tokenEntity.UserId, remainingLifetime);
            }

            return tokenEntity.UserId;
        }

        public async Task RevokeAsync(string refreshToken)
        {

            // 1. Revoke in SQL Server
            var tokenEntity = await context.RefreshTokens.IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

           
            if (tokenEntity is not null)
            {
                tokenEntity.IsRevoked = true;
                await context.SaveChangesAsync();
            }

            // 2. Remove from Redis Cache
            await cacheService.RemoveAsync(BuildKey(refreshToken));
        }

        private static string GenerateSecureToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }
    }
}