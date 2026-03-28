using System.Globalization;
using CarrierRatesQuery.Api.Services.Rates;
using CarrierRatesQuery.Api.Services.Rates.Clients;

namespace CarrierRatesQuery.Api.Services.Rates.Adapters;

public sealed class FedExRateAdapter : ICarrierRateAdapter<MockFedExRateResponse>
{
    public ShippingRateResponseDto Adapt(MockFedExRateResponse source)
    {
        var mappedOptions = source.ServiceOptions
            .Select(option => new RateOption(
                ServiceName: option.ServiceName,
                EstimatedDelivery: ParseDate(option.EstimatedDelivery),
                Price: new MoneyDto(Amount: option.Rate, Currency: "USD")))
            .ToList();

        return new ShippingRateResponseDto(
            Carrier: source.Carrier,
            RateOptions: mappedOptions);
    }

    private static DateTime ParseDate(string value)
    {
        if (DateOnly.TryParseExact(
            value,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var date))
        {
            return date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        }

        throw new FormatException($"Invalid FedEx estimated delivery date format: '{value}'.");
    }
}
