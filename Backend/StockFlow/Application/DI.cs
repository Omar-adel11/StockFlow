using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Caching;
using Application.Interfaces;
using Application.Interfaces.AuthInterfaces;
using Application.Services;
using Application.Services.Auth;
using Application.Services.Helper;
using Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Application
{
    public static class DI
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Core Domain & Application Services
            services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
            services.AddScoped<ISalesOrderService, SaleOrderService>();
            // Register core service
            services.AddScoped<CategoryService>();

            // Register decorator wrapping the core service
            services.AddScoped<ICategoryService>(provider =>
                new CachedCategoryService(
                    provider.GetRequiredService<CategoryService>(),
                    provider.GetRequiredService<ICacheService>()
                )
            );
            services.AddScoped<ProductService>();
            services.AddScoped<IProductService>(provider =>
               new CachedProductService(
                   provider.GetRequiredService<ProductService>(),
                   provider.GetRequiredService<ICacheService>()
               )
           );

            services.AddScoped<WarehouseService>();
            services.AddScoped<IWarehouseService>(provider =>
              new CachedWarehouseService(
                  provider.GetRequiredService<WarehouseService>(),
                  provider.GetRequiredService<ICacheService>()
              )
          );

            services.AddScoped<SupplierService>();
            services.AddScoped<ISupplierService>(provider =>
            new CachedSupplierService(
                provider.GetRequiredService<SupplierService>(),
                provider.GetRequiredService<ICacheService>()
            )
        );
            services.AddScoped<CustomerService>();
            services.AddScoped<ICustomerService>(provider =>
           new CachedCustomerService(
               provider.GetRequiredService<CustomerService>(),
               provider.GetRequiredService<ICacheService>()
           )
       );
            services.AddScoped<IStockMovementService, StockMovementService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<ITeamService, TeamService>();
            services.AddScoped<ISaaSAdminService, SaaSAdminService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IBusinessService, BusinessService>();
            services.AddScoped<IDashboardService, DashboardService>();

            services.AddHttpContextAccessor();

            // Auth & Auxiliary Services
            services.AddScoped<IPlanService, PlanService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<ITokenService, JWTTokenService>();
            services.AddScoped<IOTPService, OTPService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<ICacheService, RedisCacheService>();


            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<IFeatureService, FeatureService>();
            services.AddScoped<ISubscriptionService, SubscriptionService>();
            // Service Manager (Unit of Work pattern for Services)
            services.AddScoped<IServiceManager, ServiceManager>();

            // Custom Validation Response Configuration
            services.Configure<ApiBehaviorOptions>(config =>
            {
                config.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState
                        .Where(m => m.Value != null && m.Value.Errors.Any())
                        .Select(m => new ValidationError()
                        {
                            Field = m.Key,
                            Errors = m.Value!.Errors.Select(e => e.ErrorMessage)
                        });

                    var response = new ValidationErrorResponse()
                    {
                        Errors = errors
                    };

                    return new BadRequestObjectResult(response);
                };
            });

            return services;
        }
    }
}