using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Application;
using Application.Interfaces;
using Application.Services.Auth;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using StockFlow.Middlewares;


namespace StockFlow
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
            {

                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

            IWebHostEnvironment env = builder.Environment;
            //JWT
            builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JWT"));
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = builder.Configuration["JWT:Issuer"],
                    ValidAudience = builder.Configuration["JWT:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]))
                };
            });


            builder.Services.AddRateLimiter(options =>
            {
                // Return standard HTTP 429 instead of default 503
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;


                // Define a Sliding Window policy partitioned by user IP address
                options.AddPolicy("sliding-by-ip", context =>
                {
                    string clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                    return RateLimitPartition.GetSlidingWindowLimiter(clientIp, _ =>
                        new SlidingWindowRateLimiterOptions
                        {
                            PermitLimit = 5,                              // Max 5 requests
                            Window = TimeSpan.FromMinutes(1),             // Per 1 minute
                            SegmentsPerWindow = 6,
                            QueueLimit = 0                                // Reject immediately if limit is hit
                        });
                });

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    var errorResponse = new
                    {
                        success = false,
                        statusCode = StatusCodes.Status429TooManyRequests,
                        ErrorMessage = "Too many requests. Please try again later."
                    };

                    var json = JsonSerializer.Serialize(errorResponse);
                    await context.HttpContext.Response.WriteAsync(json, cancellationToken);
                };
            });



            const string FrontendCorsPolicy = "FrontendCorsPolicy";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(FrontendCorsPolicy, policy =>
                {
                    policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseMiddleware<GlobalErrorHandlingMiddleware>();

            app.UseCors(FrontendCorsPolicy);
            app.UseStaticFiles();

            app.UseRateLimiter();
            app.UseHttpsRedirection();


            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
