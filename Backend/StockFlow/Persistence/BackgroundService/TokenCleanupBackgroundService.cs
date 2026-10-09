using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Persistence.BackgroundServices
{
    public class TokenCleanupBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<TokenCleanupBackgroundService> logger) : BackgroundService
    {
        // Run cleanup every 24 hours
        private readonly TimeSpan _period = TimeSpan.FromHours(12);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("Token Cleanup Background Service starting.");

            using var timer = new PeriodicTimer(_period);

            // PeriodicTimer keeps running until the application shuts down
            while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    logger.LogInformation("Starting periodic cleanup of expired refresh tokens...");

                    // Create an explicit scope to resolve the scoped IAppDbContext
                    using var scope = serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

                    // Threshold: Delete tokens expired for more than 30 days
                    var cutoffDate = DateTime.UtcNow.AddDays(-30);

                    // Execute direct SQL batch delete (EF Core 7+) for maximum efficiency
                    var deletedCount = await context.RefreshTokens
                        .IgnoreQueryFilters()
                        .Where(t => t.ExpiresAt < cutoffDate || t.IsRevoked)
                        .ExecuteDeleteAsync(stoppingToken);

                    logger.LogInformation("Token cleanup completed successfully. Removed {Count} stale refresh tokens.", deletedCount);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while cleaning up expired refresh tokens.");
                }
            }
        }
    }
}