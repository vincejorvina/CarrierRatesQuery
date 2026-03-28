using CarrierRatesQuery.Api.Data.Entities;

namespace CarrierRatesQuery.Api.Services.Rates.Strategies;

public interface ICarrierRateStrategy
{
    string CarrierSlug { get; }
    Task<ShippingRateResponseDto?> TryGetRatesAsync(Carrier carrier, RateQueryRequest request, CancellationToken cancellationToken);
}
