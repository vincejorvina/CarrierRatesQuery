namespace CarrierRatesQuery.Api.Services.Rates;

public interface ICarrierRateAdapter<in TCarrierResponse>
{
    ShippingRateResponseDto Adapt(TCarrierResponse source);
}

public sealed record ShippingRateResponseDto(string Carrier, IReadOnlyList<RateOption> RateOptions);

public sealed record RateOption(string ServiceName, DateTime EstimatedDelivery, MoneyDto Price);

public sealed record MoneyDto(decimal Amount, string Currency);
