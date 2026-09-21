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
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IWarehouseService, WarehouseService>();
            services.AddScoped<ISupplierService, SupplierService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IStockMovementService, StockMovementService>();
            services.AddScoped<IInventoryService, InventoryService>();

            // Auth & Auxiliary Services
            services.AddScoped<IPlanService, PlanService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<ITokenService, JWTTokenService>();
            services.AddScoped<IOTPService, OTPService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<ICacheService, RedisCacheService>();

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