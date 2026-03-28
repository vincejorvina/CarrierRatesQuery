namespace CarrierRatesQuery.Api.Services.Rates;

public interface ICarrierRateAdapter<in TCarrierResponse>
{
    ShippingRateResponseDto Adapt(TCarrierResponse source);
}

/// <summary>
/// Unified shipping rate response from a carrier.
/// </summary>
/// <param name="Carrier">The carrier name (e.g. "FedEx", "UPS", "DHL").</param>
/// <param name="RateOptions">Available shipping service options and their rates.</param>
public sealed record ShippingRateResponseDto(string Carrier, IReadOnlyList<RateOption> RateOptions);

/// <summary>
/// A single shipping service option with pricing and estimated delivery.
/// </summary>
/// <param name="ServiceName">The service name (e.g. "FedEx Ground", "UPS Next Day Air").</param>
/// <param name="EstimatedDelivery">Estimated delivery date.</param>
/// <param name="Price">The shipping price.</param>
public sealed record RateOption(string ServiceName, DateTime EstimatedDelivery, MoneyDto Price);

/// <summary>
/// A monetary amount with currency.
/// </summary>
/// <param name="Amount">The monetary amount.</param>
/// <param name="Currency">The ISO 4217 currency code (e.g. "USD").</param>
public sealed record MoneyDto(decimal Amount, string Currency);
