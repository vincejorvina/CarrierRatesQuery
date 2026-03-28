namespace CarrierRatesQuery.Api.Services.Rates;

public sealed record RateQueryRequest(decimal Weight, decimal Length, decimal Width, decimal Height);
