namespace CarrierRatesQuery.Api.Services.CarrierEndpoints;

public sealed class CarrierEndpointNotFoundException(Guid carrierId, Guid endpointId)
    : Exception($"Endpoint '{endpointId}' was not found for carrier '{carrierId}'.")
{
    public Guid CarrierId { get; } = carrierId;
    public Guid EndpointId { get; } = endpointId;
}
