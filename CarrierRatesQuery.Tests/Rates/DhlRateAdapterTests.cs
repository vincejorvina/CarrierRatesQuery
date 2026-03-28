using CarrierRatesQuery.Api.Services.Rates.Adapters;
using CarrierRatesQuery.Api.Services.Rates.Clients;

namespace CarrierRatesQuery.Tests.Rates;

public class DhlRateAdapterTests
{
    [Fact]
    public void Adapt_ValidDhlResponse_ReturnsUnifiedShippingRateResponse()
    {
        // Arrange
        var adapter = new DhlRateAdapter();
        var source = new MockDhlRateResponse(
            Provider: "DHL",
            Options:
            [
                new MockDhlServiceOption("DHL Economy Select", "2026-06-16", 11.00m)
            ]);

        // Act
        var result = adapter.Adapt(source);

        // Assert
        Assert.Equal("DHL", result.Carrier);
        Assert.Single(result.RateOptions);
        Assert.Equal("DHL Economy Select", result.RateOptions[0].ServiceName);
        Assert.Equal(11.00m, result.RateOptions[0].Price.Amount);
        Assert.Equal("USD", result.RateOptions[0].Price.Currency);
    }

    [Fact]
    public void Adapt_InvalidDate_ThrowsFormatException()
    {
        // Arrange
        var adapter = new DhlRateAdapter();
        var source = new MockDhlRateResponse(
            Provider: "DHL",
            Options:
            [
                new MockDhlServiceOption("DHL Economy Select", "2026/06/16", 11.00m)
            ]);

        // Act
        Action action = () => _ = adapter.Adapt(source);

        // Assert
        Assert.Throws<FormatException>(action);
    }
}
