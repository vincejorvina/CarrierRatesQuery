namespace CarrierRatesQuery.Api.Services.Carriers;

/// <summary>
/// Request to create a new carrier.
/// </summary>
/// <param name="Name">Carrier display name (e.g. "FedEx").</param>
/// <param name="IsEnabled">Whether the carrier should be enabled immediately.</param>
public sealed record CreateCarrierRequest(string Name, bool IsEnabled);

/// <summary>
/// Request to update an existing carrier.
/// </summary>
/// <param name="Name">Updated carrier display name.</param>
/// <param name="IsEnabled">Updated enabled state.</param>
public sealed record UpdateCarrierRequest(string Name, bool IsEnabled);

/// <summary>
/// Request to disable a carrier. A reason is required and will be logged.
/// </summary>
/// <param name="Reason">The reason for disabling (e.g. "maintenance", "contract termination").</param>
public sealed record DisableCarrierRequest(string Reason);

/// <summary>
/// Carrier response with configuration details.
/// </summary>
/// <param name="Id">Unique identifier.</param>
/// <param name="Name">Display name.</param>
/// <param name="Slug">URL-friendly identifier derived from the name.</param>
/// <param name="IsEnabled">Whether the carrier is currently active.</param>
/// <param name="CreatedAtUtc">When the carrier was created.</param>
/// <param name="UpdatedAtUtc">When the carrier was last updated, or null if never modified.</param>
/// <param name="EndPoints">Configured API endpoints for this carrier.</param>
public sealed record CarrierResponseDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsEnabled,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyList<CarrierEndpointDto> EndPoints
);

/// <summary>
/// A configured API endpoint for a carrier.
/// </summary>
/// <param name="Id">Unique identifier.</param>
/// <param name="Operation">The operation type (e.g. "Rates").</param>
/// <param name="Endpoint">The endpoint URL.</param>
public sealed record CarrierEndpointDto(Guid Id, string Operation, string Endpoint);
