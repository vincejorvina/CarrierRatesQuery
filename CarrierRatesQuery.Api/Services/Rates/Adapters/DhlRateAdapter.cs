using System.Globalization;
using CarrierRatesQuery.Api.Services.Rates;
using CarrierRatesQuery.Api.Services.Rates.Clients;

namespace CarrierRatesQuery.Api.Services.Rates.Adapters;

public sealed class DhlRateAdapter : ICarrierRateAdapter<MockDhlRateResponse>
{
    public ShippingRateResponseDto Adapt(MockDhlRateResponse source)
    {
        var mappedOptions = source.Options
            .Select(option => new RateOption(
                ServiceName: option.Name,
                EstimatedDelivery: ParseDate(option.DeliveryDate),
                Price: new MoneyDto(Amount: option.Price, Currency: "USD")))
            .ToList();

        return new ShippingRateResponseDto(
            Carrier: source.Provider,
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

        throw new FormatException($"Invalid DHL estimated delivery date format: '{value}'.");
    }
}
