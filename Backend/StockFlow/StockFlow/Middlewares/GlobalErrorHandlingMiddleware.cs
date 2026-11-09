using System.Security.Claims;
using System.Text.Json;
using Domain.Exceptions;
using Domain.Exceptions.BadRequest;
using Domain.Exceptions.NotFound;
using Domain.Exceptions.Unauthorized;
using Serilog.Context;

namespace StockFlow.Middlewares
{
    public class GlobalErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalErrorHandlingMiddleware> _logger;

        public GlobalErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
                await HandlingNotFoundErrorAsync(context);
            }
            catch (Exception ex)
            {
                var userId = context.User
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var businessId = context.User
                    .FindFirst("business_id")?.Value;

                using (LogContext.PushProperty("UserId", userId ?? "Anonymous"))
                using (LogContext.PushProperty("BusinessId", businessId ?? "None"))
                {

                    if (context.Response.HasStarted)
                    {
                        _logger.LogWarning(
                            "The response has already started. The exception response cannot be written.");

                        throw;
                    }

                    //Log the exception (you can use any logging framework here)
                    await HandleExceptionAsync(context, ex);
                }
            }
        }

        private async Task HandleExceptionAsync(
    HttpContext context,
    Exception exception)
        {
            var statusCode = exception switch
            {
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                NotFoundException => StatusCodes.Status404NotFound,
                BadRequestException => StatusCodes.Status400BadRequest,
                InvalidOperationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "An unexpected exception occurred. StatusCode: {StatusCode}, Method: {RequestMethod}, Path: {RequestPath}",
                    statusCode,
                    context.Request.Method,
                    context.Request.Path);
            }
            else
            {
                _logger.LogWarning(
                    "Request failed with status {StatusCode}: {ExceptionMessage}",
                    statusCode,
                    exception.Message);
            }

            var response = new ErrorDetails
            {
                StatusCode = statusCode,
                ErrorMessage = exception switch
                {
                    UnauthorizedException or
                    NotFoundException or
                    BadRequestException or
                    RegisterationBadRequestException or
                    InvalidOperationException => exception.Message,

                    _ => "Internal Server Error. Please try again later."
                }
            };

            context.Response.Clear();
            context.Response.StatusCode = response.StatusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }

        private static async Task HandlingNotFoundErrorAsync(HttpContext context)
        {
            if (context.Response.StatusCode == StatusCodes.Status404NotFound)
            {
                context.Response.ContentType = "application/json";
                var respone = new ErrorDetails()
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    ErrorMessage = $"End point {context.Request.Path} is not found"
                };
                var json = JsonSerializer.Serialize(respone);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
