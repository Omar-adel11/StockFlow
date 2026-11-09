using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Exceptions.NotFound;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using StockFlow.Middlewares;

namespace StockFlow.tests.Handlers
{
    public class GlobalErrorHandlingMiddlewaretests
    {
        [Fact]
        public async Task InvokeAsync_ProductNotFound_Returns404()
        {
            // Arrange
            RequestDelegate next = context =>
                throw new ProductNotFoundException();
            var mockLogger = new Mock<ILogger<GlobalErrorHandlingMiddleware>>();

            var middleware = new GlobalErrorHandlingMiddleware(next,mockLogger.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(
                StatusCodes.Status404NotFound,
                context.Response.StatusCode);
        }
    }
}
