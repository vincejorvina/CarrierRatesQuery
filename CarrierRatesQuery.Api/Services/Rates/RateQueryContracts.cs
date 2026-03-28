namespace CarrierRatesQuery.Api.Services.Rates;

/// <summary>
/// Package details for a rate query.
/// </summary>
/// <param name="Weight">Package weight.</param>
/// <param name="Length">Package length.</param>
/// <param name="Width">Package width.</param>
/// <param name="Height">Package height.</param>
public sealed record RateQueryRequest(decimal Weight, decimal Length, decimal Width, decimal Height);
