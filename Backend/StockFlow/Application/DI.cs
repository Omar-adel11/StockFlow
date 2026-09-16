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
            services.AddScoped<IPlanService, PlanService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<ITokenService, JWTTokenService>();
            services.AddScoped<IOTPService, OTPService>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IServiceManager, ServiceManager>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<ICacheService, RedisCacheService>();

            services.Configure<ApiBehaviorOptions>(config =>
            {
                config.InvalidModelStateResponseFactory = (actionContext) =>
                {
                    var errors = actionContext.ModelState.Where(m => m.Value.Errors.Any())
                                                            .Select(m => new ValidationError()
                                                            {
                                                                Field = m.Key,
                                                                Errors = m.Value.Errors.Select(errors => errors.ErrorMessage)
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
