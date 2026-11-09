using System.Text;
using System.Text.Json;
using System.Threading.RateLimiting;
using Application;
using Application.Interfaces;
using Application.Services.Auth;
using Domain.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Persistence.Interceptors;
using Persistence.Seed;
using Serilog;
using Serilog.Events;
using StockFlow.Middlewares;


namespace StockFlow
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                            .WriteTo.Console()
                            .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting StockFlow API");

                var builder = WebApplication.CreateBuilder(args);

                //Add support to logging with serilog
                builder.Host.UseSerilog((context, services, configuration) =>
                {
                    configuration.ReadFrom.Configuration(context.Configuration); 
                });

                // Add services to the container.

                builder.Services.AddControllers();
                // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                builder.Services.AddApplicationServices();
                builder.Services.AddInfrastructureServices(builder.Configuration);
                // 3. Register ApplicationDbContext with Interceptors


                builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
                {

                    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ ";
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

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
                                PermitLimit = 10,                              // Max 10 requests
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
                        policy.WithOrigins("http://127.0.0.1:5501", "http://localhost:5501")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                             .AllowCredentials();
                    });
                });


                var app = builder.Build();

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var context = services.GetRequiredService<AppDbContext>();

                    // This applies all pending migrations automatically on cloud startup!
                    await context.Database.MigrateAsync();

                    await DbSeeder.SeedAsync(services);

                }
                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseSerilogRequestLogging(options =>
                {
                    options.GetLevel = (httpContext, elapsed, ex) =>
                        httpContext.Request.Method == "OPTIONS"
                            ? LogEventLevel.Verbose
                            : LogEventLevel.Information;
                });
                app.UseMiddleware<GlobalErrorHandlingMiddleware>();

                app.UseCors(FrontendCorsPolicy);
                app.UseHttpsRedirection();
                app.UseStaticFiles();
                app.UseRateLimiter();
                app.UseAuthentication();
                app.UseMiddleware<SerilogContextMiddleware>();
                app.UseAuthorization();
                app.MapControllers();
                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "StockFlow API terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}


/*
 * 
 * 
 * 14. Authentication and Authorization

Important events:

Successful authentication.
Failed authentication.
Account deactivation.
Unauthorized access attempts.
Permission failures.
Refresh token failures.

Security warning:

Never log:

Passwords.
JWT access tokens.
Refresh tokens.
Authentication cookies.
OTP values.

You can log the UserId and relevant operation information when appropriate.

15. Multi-Tenancy

This is particularly important in StockFlow.

You have tenant isolation through BusinessId claims and global query filters.

Your logs should provide enough context to investigate tenant-related issues.

For example:

A product operation fails.

You should be able to identify:

UserId.
BusinessId.
ProductId.
Operation.

This helps investigate tenant isolation and authorization issues.

However, be careful not to log sensitive customer information.

16. Inventory Management

Important events:

Stock adjustment.
Warehouse transfer.
Inventory update failures.
Insufficient stock.
Failed stock reservation.

For example, when transferring products between warehouses, useful contextual properties include:

BusinessId.
ProductId.
SourceWarehouseId.
DestinationWarehouseId.
Quantity.

This makes investigating inventory discrepancies much easier.

17. Subscription Management

Your subscription module has important business operations.

Log events such as:

Subscription created.
Subscription deactivated.
Plan changed.
Subscription activation failed.
Free trial activated.

Useful properties:

BusinessId.
SubscriptionId.
PlanId.
Operation type.
18. Paymob Payments

This module deserves special attention.

Important events:

Payment initiation.
Payment verification.
Webhook received.
Webhook signature validation failed.
Subscription activation after successful payment.
Payment processing exception.

You should record transaction identifiers and relevant processing outcomes.

Never log:

Secret keys.
API credentials.
Payment authentication tokens.
Sensitive payment details.
Full webhook signatures.

Remember that webhook endpoints can receive repeated notifications.

Structured logs help you investigate duplicate processing and idempotency problems.

19. Redis

You use Redis for caching and refresh-token management.

Useful events:

Cache miss.
Cache invalidation.
Redis connection failure.
Refresh token revoked.
Cache operation failure.

However, don't log every cache hit.

That can create enormous amounts of unnecessary logging.

Prefer logging meaningful events, failures, and selected diagnostic information.
 */