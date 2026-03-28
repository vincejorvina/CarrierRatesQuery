using System.Globalization;
using CarrierRatesQuery.Api.Services.Rates;
using CarrierRatesQuery.Api.Services.Rates.Clients;

namespace CarrierRatesQuery.Api.Services.Rates.Adapters;

public sealed class UpsRateAdapter : ICarrierRateAdapter<MockUpsRateResponse>
{
    public ShippingRateResponseDto Adapt(MockUpsRateResponse source)
    {
        var mappedOptions = source.Services
            .Select(option => new RateOption(
                ServiceName: option.Service,
                EstimatedDelivery: ParseDate(option.Eta),
                Price: new MoneyDto(Amount: option.Cost, Currency: "USD")))
            .ToList();

        return new ShippingRateResponseDto(
            Carrier: source.Company,
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

        throw new FormatException($"Invalid UPS estimated delivery date format: '{value}'.");
    }
}
