using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Email;
using Persistence.Repositories;

namespace Persistence
{
    // Keeps Program.cs clean - as the project grows, every layer can
    // expose one "AddXServices" extension like this one.
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

            services.AddScoped<IContactRepository, ContactRepository>();
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}
