using CarrierRatesQuery.Api.Services.Rates.Adapters;
using CarrierRatesQuery.Api.Services.Rates.Clients;

namespace CarrierRatesQuery.Tests.Rates;

public class FedExRateAdapterTests
{
    [Fact]
    public void Adapt_ValidFedExResponse_ReturnsUnifiedShippingRateResponse()
    {
        // Arrange
        var adapter = new FedExRateAdapter();

        var source = new MockFedExRateResponse(
            Carrier: "FedEx",
            ServiceOptions:
            [
                new MockFedExServiceOption("FedEx Ground", "2026-06-15", 12.34m)
            ]);

        // Act
        var result = adapter.Adapt(source);

        // Assert
        Assert.Equal("FedEx", result.Carrier);
        Assert.Single(result.RateOptions);

        var option = result.RateOptions[0];
        Assert.Equal("FedEx Ground", option.ServiceName);
        Assert.Equal(new DateTime(2026, 6, 15, 0, 0, 0, DateTimeKind.Utc), option.EstimatedDelivery);
        Assert.Equal(12.34m, option.Price.Amount);
        Assert.Equal("USD", option.Price.Currency);
    }

    [Fact]
    public void Adapt_MultipleServiceOptions_MapsAllOptions()
    {
        // Arrange
        var adapter = new FedExRateAdapter();

        var source = new MockFedExRateResponse(
            Carrier: "FedEx",
            ServiceOptions:
            [
                new MockFedExServiceOption("FedEx Ground", "2026-06-15", 12.34m),
                new MockFedExServiceOption("FedEx Express", "2026-06-10", 24.99m),
                new MockFedExServiceOption("FedEx Overnight", "2026-06-08", 45.00m)
            ]);

        // Act
        var result = adapter.Adapt(source);

        // Assert
        Assert.Equal(3, result.RateOptions.Count);
        Assert.Equal("FedEx Ground", result.RateOptions[0].ServiceName);
        Assert.Equal(12.34m, result.RateOptions[0].Price.Amount);
        Assert.Equal("FedEx Express", result.RateOptions[1].ServiceName);
        Assert.Equal(24.99m, result.RateOptions[1].Price.Amount);
        Assert.Equal("FedEx Overnight", result.RateOptions[2].ServiceName);
        Assert.Equal(45.00m, result.RateOptions[2].Price.Amount);
    }

    [Fact]
    public void Adapt_EmptyServiceOptions_ReturnsEmptyRateOptions()
    {
        // Arrange
        var adapter = new FedExRateAdapter();
        var source = new MockFedExRateResponse(Carrier: "FedEx", ServiceOptions: []);

        // Act
        var result = adapter.Adapt(source);

        // Assert
        Assert.Equal("FedEx", result.Carrier);
        Assert.Empty(result.RateOptions);
    }

    [Fact]
    public void Adapt_InvalidDate_ThrowsFormatException()
    {
        // Arrange
        var adapter = new FedExRateAdapter();
        var source = new MockFedExRateResponse(
            Carrier: "FedEx",
            ServiceOptions:
            [
                new MockFedExServiceOption("FedEx Ground", "06/15/2026", 12.34m)
            ]);

        // Act
        Action action = () => _ = adapter.Adapt(source);

        // Assert
        Assert.Throws<FormatException>(action);
    }
}
