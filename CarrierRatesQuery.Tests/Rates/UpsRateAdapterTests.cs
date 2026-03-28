using CarrierRatesQuery.Api.Services.Rates.Adapters;
using CarrierRatesQuery.Api.Services.Rates.Clients;

namespace CarrierRatesQuery.Tests.Rates;

public class UpsRateAdapterTests
{
    [Fact]
    public void Adapt_ValidUpsResponse_ReturnsUnifiedShippingRateResponse()
    {
        // Arrange
        var adapter = new UpsRateAdapter();
        var source = new MockUpsRateResponse(
            Company: "UPS",
            Services:
            [
                new MockUpsServiceOption("UPS Ground", "2026-06-15", 15.20m)
            ]);

        // Act
        var result = adapter.Adapt(source);

        // Assert
        Assert.Equal("UPS", result.Carrier);
        Assert.Single(result.RateOptions);
        Assert.Equal("UPS Ground", result.RateOptions[0].ServiceName);
        Assert.Equal(15.20m, result.RateOptions[0].Price.Amount);
        Assert.Equal("USD", result.RateOptions[0].Price.Currency);
    }

    [Fact]
    public void Adapt_InvalidDate_ThrowsFormatException()
    {
        // Arrange
        var adapter = new UpsRateAdapter();
        var source = new MockUpsRateResponse(
            Company: "UPS",
            Services:
            [
                new MockUpsServiceOption("UPS Ground", "06-15-2026", 15.20m)
            ]);

        // Act
        Action action = () => _ = adapter.Adapt(source);

        // Assert
        Assert.Throws<FormatException>(action);
    }
}
