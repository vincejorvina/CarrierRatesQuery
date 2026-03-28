namespace CarrierRatesQuery.Api.Services.CarrierEndpoints;

/// <summary>
/// Request to create a new carrier endpoint.
/// </summary>
/// <param name="Operation">The operation type (e.g. "Rates").</param>
/// <param name="Endpoint">The endpoint URL.</param>
public sealed record CreateCarrierEndpointRequest(string Operation, string Endpoint);

/// <summary>
/// Request to update an existing carrier endpoint.
/// </summary>
/// <param name="Operation">Updated operation type.</param>
/// <param name="Endpoint">Updated endpoint URL.</param>
public sealed record UpdateCarrierEndpointRequest(string Operation, string Endpoint);

/// <summary>
/// Carrier endpoint response.
/// </summary>
/// <param name="Id">Unique identifier.</param>
/// <param name="CarrierId">The parent carrier's identifier.</param>
/// <param name="Operation">The operation type.</param>
/// <param name="Endpoint">The endpoint URL.</param>
public sealed record CarrierEndpointResponseDto(Guid Id, Guid CarrierId, string Operation, string Endpoint);
