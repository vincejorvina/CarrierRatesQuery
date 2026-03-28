namespace CarrierRatesQuery.Api.Infrastructure;

public enum RequestRole
{
    User = 0,
    Admin = 1
}

public interface IRequestRoleAccessor
{
    RequestRole CurrentRole { get; }
    bool IsAdmin { get; }
    RequestRole GetRequiredRole();
    string GetRequestedBy();
}

public sealed class RequestRoleAccessor(IHttpContextAccessor httpContextAccessor) : IRequestRoleAccessor
{
    private const string RoleHeaderName = "X-Role";
    private const string RequestedByHeaderName = "X-Requested-By";

    public RequestRole CurrentRole
    {
        get
        {
            var context = httpContextAccessor.HttpContext;
            if (context is null)
            {
                return RequestRole.User;
            }

            if (!context.Request.Headers.TryGetValue(RoleHeaderName, out var value))
            {
                return RequestRole.User;
            }

            return value.ToString().Equals("Admin", StringComparison.OrdinalIgnoreCase)
                ? RequestRole.Admin
                : RequestRole.User;
        }
    }

    public bool IsAdmin => CurrentRole == RequestRole.Admin;

    public RequestRole GetRequiredRole()
    {
        var context = httpContextAccessor.HttpContext
            ?? throw new MissingRequestHeaderException(RoleHeaderName);

        if (!context.Request.Headers.TryGetValue(RoleHeaderName, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new MissingRequestHeaderException(RoleHeaderName);
        }

        var roleValue = value.ToString().Trim();
        if (roleValue.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            return RequestRole.Admin;
        }

        if (roleValue.Equals("User", StringComparison.OrdinalIgnoreCase))
        {
            return RequestRole.User;
        }

        throw new InvalidRequestHeaderException(RoleHeaderName, "supported values are 'Admin' or 'User'.");
    }

    public string GetRequestedBy()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null)
        {
            return "unknown";
        }

        if (context.Request.Headers.TryGetValue(RequestedByHeaderName, out var value) && !string.IsNullOrWhiteSpace(value))
        {
            return value.ToString().Trim();
        }

        var role = GetRequiredRole();
        return role.ToString().ToLowerInvariant();
    }
}
