namespace CarrierRatesQuery.Api.Services.Carriers;

public sealed record CreateCarrierRequest(string Name, bool IsEnabled);

public sealed record UpdateCarrierRequest(string Name, bool IsEnabled);

public sealed record DisableCarrierRequest(string Reason);

public sealed record CarrierResponseDto(
    Guid Id,
    string Name,
    string Slug,
    bool IsEnabled,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyList<CarrierEndpointDto> Endpoints
);

public sealed record CarrierEndpointDto(Guid Id, string Operation, string Endpoint);
