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
}
