using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.Rates;
using CarrierRatesQuery.Api.Services.Rates.Strategies;

namespace CarrierRatesQuery.Tests.Rates;

public class CarrierRateStrategyResolverTests
{
    [Fact]
    public void TryResolve_ExistingSlug_ReturnsMatchingStrategy()
    {
        // Arrange
        var fedExStrategy = new FakeStrategy("fedex");
        var resolver = new CarrierRateStrategyResolver([fedExStrategy, new FakeStrategy("ups")]);

        // Act
        var found = resolver.TryResolve("fedex", out var strategy);

        // Assert
        Assert.True(found);
        Assert.Same(fedExStrategy, strategy);
    }

    [Fact]
    public void TryResolve_UnknownSlug_ReturnsFalse()
    {
        // Arrange
        var resolver = new CarrierRateStrategyResolver([new FakeStrategy("fedex")]);

        // Act
        var found = resolver.TryResolve("dhl", out _);

        // Assert
        Assert.False(found);
    }

    [Fact]
    public void TryResolve_DuplicateSlug_UsesFirstRegisteredStrategy()
    {
        // Arrange
        var first = new FakeStrategy("fedex");
        var second = new FakeStrategy("fedex");
        var resolver = new CarrierRateStrategyResolver([first, second]);

        // Act
        var found = resolver.TryResolve("fedex", out var strategy);

        // Assert
        Assert.True(found);
        Assert.Same(first, strategy);
    }

    private sealed class FakeStrategy(string carrierSlug) : ICarrierRateStrategy
    {
        public string CarrierSlug { get; } = carrierSlug;

        public Task<ShippingRateResponseDto?> TryGetRatesAsync(
            Carrier carrier,
            RateQueryRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<ShippingRateResponseDto?>(null);
        }
    }
}
