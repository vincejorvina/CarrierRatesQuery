namespace CarrierRatesQuery.Api.Services.CarrierEndpoints;

public sealed record CreateCarrierEndpointRequest(string Operation, string Endpoint);

public sealed record UpdateCarrierEndpointRequest(string Operation, string Endpoint);

public sealed record CarrierEndpointResponseDto(Guid Id, Guid CarrierId, string Operation, string Endpoint);
