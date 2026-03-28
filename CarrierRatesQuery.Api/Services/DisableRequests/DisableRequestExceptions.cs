namespace CarrierRatesQuery.Api.Services.DisableRequests;

public sealed class DisableRequestNotFoundException(Guid disableRequestId)
    : Exception($"Disable request '{disableRequestId}' was not found.")
{
    public Guid DisableRequestId { get; } = disableRequestId;
}
