using Application.Interfaces;
using Application.Interfaces.AuthInterfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Email;
using Persistence.Repositories;
using Persistence.Repository;
using StackExchange.Redis;
namespace Persistence
{
    // Keeps Program.cs clean - as the project grows, every layer can
    // expose one "AddXServices" extension like this one.
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<IAppDbContext, AppDbContext>(options =>
                     options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();


            var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));

            services.AddScoped<IPlanRepository, PlanRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ICacheRepository, CacheRepository>();

            return services;
        }
    }
}
