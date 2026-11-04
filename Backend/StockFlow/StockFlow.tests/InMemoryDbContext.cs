using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using System.Security.Claims;

public class InMemoryDbContext : AppDbContext
{
    public InMemoryDbContext()
        : base(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options,
            GetMockHttpContextAccessor())
    {
    }

    private static IHttpContextAccessor GetMockHttpContextAccessor()
    {
        var mockAccessor = new Mock<IHttpContextAccessor>();
        var context = new DefaultHttpContext();

        // This injects the business_id claim so CurrentBusinessId returns 1
        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[]
            {
                new Claim("business_id", "1")
            }, "TestAuth")
        );

        mockAccessor.Setup(a => a.HttpContext).Returns(context);
        return mockAccessor.Object;
    }

    public override void Dispose()
    {
        Database.EnsureDeleted();
        base.Dispose();
    }
}