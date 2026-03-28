using CarrierRatesQuery.Api.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace CarrierRatesQuery.Tests.Infrastructure;

public class RequestRoleAccessorTests
{
    [Fact]
    public void CurrentRole_NoHeader_ReturnsUser()
    {
        // Arrange
        var contextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        var roleAccessor = new RequestRoleAccessor(contextAccessor);

        // Act
        var role = roleAccessor.CurrentRole;

        // Assert
        Assert.Equal(RequestRole.User, role);
        Assert.False(roleAccessor.IsAdmin);
    }

    [Fact]
    public void CurrentRole_AdminHeader_ReturnsAdmin()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Role"] = "Admin";

        var contextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };
        var roleAccessor = new RequestRoleAccessor(contextAccessor);

        // Act
        var role = roleAccessor.CurrentRole;

        // Assert
        Assert.Equal(RequestRole.Admin, role);
        Assert.True(roleAccessor.IsAdmin);
    }

    [Fact]
    public void GetRequiredRole_HeaderMissing_ThrowsMissingRequestHeaderException()
    {
        // Arrange
        var contextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext()
        };
        var roleAccessor = new RequestRoleAccessor(contextAccessor);

        // Act
        Action action = () => _ = roleAccessor.GetRequiredRole();

        // Assert
        Assert.Throws<MissingRequestHeaderException>(action);
    }

    [Fact]
    public void GetRequiredRole_InvalidValue_ThrowsInvalidRequestHeaderException()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Role"] = "SuperAdmin";

        var contextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };
        var roleAccessor = new RequestRoleAccessor(contextAccessor);

        // Act
        Action action = () => _ = roleAccessor.GetRequiredRole();

        // Assert
        Assert.Throws<InvalidRequestHeaderException>(action);
    }
}
