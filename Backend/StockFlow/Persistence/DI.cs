using Application.Interfaces;
using Application.Interfaces.AuthInterfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence.BackgroundServices;
using Persistence.Email;
using Persistence.Interceptors;
using Persistence.Payments;
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
            services.AddHttpContextAccessor();
            services.AddScoped<SoftDeleteInterceptor>();
            services.AddDbContext<IAppDbContext, AppDbContext>((sp, options) =>
            {
                var softDeleteInterceptor = sp.GetRequiredService<SoftDeleteInterceptor>();

                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                       .AddInterceptors(softDeleteInterceptor);
            });


            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();


            var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

            var configurationOptions = ConfigurationOptions.Parse(redisConnectionString);
            configurationOptions.AbortOnConnectFail = false;

            services.AddSingleton<IConnectionMultiplexer>(sp =>
                ConnectionMultiplexer.Connect(configurationOptions));

            services.AddScoped<IPlanRepository, PlanRepository>();
            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<ICacheRepository, CacheRepository>();
            services.AddScoped<IBusinessRepository, BusinessRepository>();

            services.AddHttpClient<PaymobPaymentGateway>();
            services.AddScoped<IPaymentGateway>(sp => sp.GetRequiredService<PaymobPaymentGateway>());
            services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
            services.AddSingleton<ILogger>(sp =>sp.GetRequiredService<ILoggerFactory>().CreateLogger("App"));

            services.AddHostedService<TokenCleanupBackgroundService>();






            return services;
        }
    }
}
