using CarrierRatesQuery.Api.Infrastructure;
using Microsoft.AspNetCore.Http;

namespace CarrierRatesQuery.Tests.Infrastructure;

public class RequestRoleAccessorTests
{
    [Fact]
    public void CurrentRole_NoHeader_ReturnsUser()
    {
        // Arrange
        var roleAccessor = CreateAccessor();

        // Act
        var role = roleAccessor.CurrentRole;

        // Assert
        Assert.Equal(RequestRole.User, role);
        Assert.False(roleAccessor.IsAdmin);
    }

    [Theory]
    [InlineData("Admin", RequestRole.Admin, true)]
    [InlineData("admin", RequestRole.Admin, true)]
    [InlineData("User", RequestRole.User, false)]
    [InlineData("SuperAdmin", RequestRole.User, false)]
    public void CurrentRole_RoleHeaderProvided_ReturnsExpectedRole(string roleHeader, RequestRole expectedRole, bool expectedIsAdmin)
    {
        // Arrange
        var roleAccessor = CreateAccessor(role: roleHeader);

        // Act
        var role = roleAccessor.CurrentRole;

        // Assert
        Assert.Equal(expectedRole, role);
        Assert.Equal(expectedIsAdmin, roleAccessor.IsAdmin);
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
        var roleAccessor = CreateAccessor(role: "SuperAdmin");

        // Act
        Action action = () => _ = roleAccessor.GetRequiredRole();

        // Assert
        Assert.Throws<InvalidRequestHeaderException>(action);
    }

    [Theory]
    [InlineData("Admin", RequestRole.Admin)]
    [InlineData("User", RequestRole.User)]
    public void GetRequiredRole_ValidHeader_ReturnsExpectedRole(string roleHeader, RequestRole expectedRole)
    {
        // Arrange
        var roleAccessor = CreateAccessor(role: roleHeader);

        // Act
        var role = roleAccessor.GetRequiredRole();

        // Assert
        Assert.Equal(expectedRole, role);
    }

    [Fact]
    public void GetRequestedBy_HeaderProvided_ReturnsHeaderValue()
    {
        // Arrange
        var roleAccessor = CreateAccessor(role: "User", requestedBy: "user.demo");

        // Act
        var requestedBy = roleAccessor.GetRequestedBy();

        // Assert
        Assert.Equal("user.demo", requestedBy);
    }

    [Fact]
    public void GetRequestedBy_HeaderMissing_ReturnsRoleAsFallback()
    {
        // Arrange
        var roleAccessor = CreateAccessor(role: "Admin");

        // Act
        var requestedBy = roleAccessor.GetRequestedBy();

        // Assert
        Assert.Equal("admin", requestedBy);
    }

    private static RequestRoleAccessor CreateAccessor(string? role = null, string? requestedBy = null)
    {
        var httpContext = new DefaultHttpContext();
        if (role is not null)
        {
            httpContext.Request.Headers["X-Role"] = role;
        }

        if (requestedBy is not null)
        {
            httpContext.Request.Headers["X-Requested-By"] = requestedBy;
        }

        return new RequestRoleAccessor(new HttpContextAccessor
        {
            HttpContext = httpContext
        });
    }
}
