namespace CarrierRatesQuery.Api.Infrastructure;

public sealed class ForbiddenOperationException(string message) : Exception(message)
{
}

public sealed class MissingRequestHeaderException(string headerName)
    : Exception($"Missing required header '{headerName}'.")
{
    public string HeaderName { get; } = headerName;
}

public sealed class InvalidRequestHeaderException(string headerName, string detail)
    : Exception($"Invalid value for header '{headerName}': {detail}")
{
    public string HeaderName { get; } = headerName;
}
