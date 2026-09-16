using System.Text.Json;
using Domain.Exceptions;
using Domain.Exceptions.BadRequest;
using Domain.Exceptions.NotFound;
using Domain.Exceptions.Unauthorized;

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
                //Log the exception (you can use any logging framework here)
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(
    HttpContext context,
    Exception exception)
        {
            _logger.LogError(
                exception,
                "An unhandled exception occurred.");

            context.Response.ContentType = "application/json";


            var respone = new ErrorDetails()
            {
                StatusCode = exception switch
                {
                    InvalidCredentialsException =>
                    StatusCodes.Status401Unauthorized,
                    InvalidOldPasswordException =>
                    StatusCodes.Status401Unauthorized,

                    UserNotFoundException =>
                    StatusCodes.Status404NotFound,

                    RegisterationBadRequestException =>
                    StatusCodes.Status400BadRequest,
                    ResetPasswordBadRequestException =>
                    StatusCodes.Status400BadRequest,

                    _ =>
                    StatusCodes.Status500InternalServerError
                },
                ErrorMessage = exception switch
                {
                    UnauthorizedException or
                    NotFoundException or
                    BadRequestException or
                    RegisterationBadRequestException => exception.Message,
                    _ => "Internal Server Error. Please try again later."
                }
            };

            context.Response.StatusCode = respone.StatusCode;
            var json = JsonSerializer.Serialize(respone);
            await context.Response.WriteAsync(json);
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
