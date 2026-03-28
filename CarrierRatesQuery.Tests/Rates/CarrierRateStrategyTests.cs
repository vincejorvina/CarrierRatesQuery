using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.Rates;
using CarrierRatesQuery.Api.Services.Rates.Adapters;
using CarrierRatesQuery.Api.Services.Rates.Clients;
using CarrierRatesQuery.Api.Services.Rates.Strategies;

namespace CarrierRatesQuery.Tests.Rates;

public class CarrierRateStrategyTests
{
    [Fact]
    public async Task FedExRateStrategy_NoRatesEndpoint_ReturnsNull()
    {
        // Arrange
        var strategy = new FedExRateStrategy(new FakeFedExClient(), new FedExRateAdapter());
        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "FedEx",
            Endpoints = []
        };

        // Act
        var result = await strategy.TryGetRatesAsync(carrier, new RateQueryRequest(5m, 10m, 5m, 5m), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpsRateStrategy_ValidRequest_MapsWeightAndDimensionsToClientPayload()
    {
        // Arrange
        var fakeClient = new CapturingUpsClient();
        var strategy = new UpsRateStrategy(fakeClient, new UpsRateAdapter());
        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "UPS",
            Endpoints =
            [
                new CarrierEndpoint
                {
                    Id = Guid.NewGuid(),
                    Operation = "Rates",
                    Endpoint = "http://localhost:5104/api/ups/shipping-rates"
                }
            ]
        };
        var request = new RateQueryRequest(8m, 20m, 11m, 7m);

        // Act
        _ = await strategy.TryGetRatesAsync(carrier, request, CancellationToken.None);

        // Assert
        Assert.NotNull(fakeClient.LastRequest);
        Assert.Equal(8m, fakeClient.LastRequest!.Shipment.WeightLbs);
        Assert.Equal(20m, fakeClient.LastRequest.Shipment.DimensionsInches.Length);
        Assert.Equal(11m, fakeClient.LastRequest.Shipment.DimensionsInches.Width);
        Assert.Equal(7m, fakeClient.LastRequest.Shipment.DimensionsInches.Height);
    }

    [Fact]
    public async Task DhlRateStrategy_ValidRequest_MapsWeightAndDimensionsToClientPayload()
    {
        // Arrange
        var fakeClient = new CapturingDhlClient();
        var strategy = new DhlRateStrategy(fakeClient, new DhlRateAdapter());
        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "DHL",
            Endpoints =
            [
                new CarrierEndpoint
                {
                    Id = Guid.NewGuid(),
                    Operation = "Rates",
                    Endpoint = "http://localhost:5204/api/dhl/rates"
                }
            ]
        };
        var request = new RateQueryRequest(9m, 15m, 14m, 6m);

        // Act
        _ = await strategy.TryGetRatesAsync(carrier, request, CancellationToken.None);

        // Assert
        Assert.NotNull(fakeClient.LastRequest);
        Assert.Equal(9m, fakeClient.LastRequest!.Parcel.WeightKg);
        Assert.Equal(15m, fakeClient.LastRequest.Parcel.SizeCm.Length);
        Assert.Equal(14m, fakeClient.LastRequest.Parcel.SizeCm.Width);
        Assert.Equal(6m, fakeClient.LastRequest.Parcel.SizeCm.Height);
    }

    [Fact]
    public async Task UpsRateStrategy_NoRatesEndpoint_ReturnsNull()
    {
        // Arrange
        var strategy = new UpsRateStrategy(new FakeUpsClient(), new UpsRateAdapter());
        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "UPS",
            Endpoints = []
        };

        // Act
        var result = await strategy.TryGetRatesAsync(carrier, new RateQueryRequest(5m, 10m, 5m, 5m), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task DhlRateStrategy_NoRatesEndpoint_ReturnsNull()
    {
        // Arrange
        var strategy = new DhlRateStrategy(new FakeDhlClient(), new DhlRateAdapter());
        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "DHL",
            Endpoints = []
        };

        // Act
        var result = await strategy.TryGetRatesAsync(carrier, new RateQueryRequest(5m, 10m, 5m, 5m), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task FedExRateStrategy_ValidRequest_ReturnsAdaptedRateResponse()
    {
        // Arrange
        var strategy = new FedExRateStrategy(new FakeFedExClient(), new FedExRateAdapter());
        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "FedEx",
            Endpoints =
            [
                new CarrierEndpoint
                {
                    Id = Guid.NewGuid(),
                    Operation = "Rates",
                    Endpoint = "http://localhost:5133/api/fedex/rates"
                }
            ]
        };

        // Act
        var result = await strategy.TryGetRatesAsync(carrier, new RateQueryRequest(5m, 10m, 5m, 5m), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("FedEx", result.Carrier);
        Assert.Single(result.RateOptions);
        Assert.Equal("FedEx Ground", result.RateOptions[0].ServiceName);
    }

    [Fact]
    public async Task UpsRateStrategy_ValidRequest_ReturnsAdaptedRateResponse()
    {
        // Arrange
        var strategy = new UpsRateStrategy(new FakeUpsClient(), new UpsRateAdapter());
        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "UPS",
            Endpoints =
            [
                new CarrierEndpoint
                {
                    Id = Guid.NewGuid(),
                    Operation = "Rates",
                    Endpoint = "http://localhost:5200/api/ups/rates"
                }
            ]
        };

        // Act
        var result = await strategy.TryGetRatesAsync(carrier, new RateQueryRequest(5m, 10m, 5m, 5m), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("UPS", result.Carrier);
        Assert.Single(result.RateOptions);
        Assert.Equal("UPS Ground", result.RateOptions[0].ServiceName);
    }

    [Fact]
    public async Task DhlRateStrategy_ValidRequest_ReturnsAdaptedRateResponse()
    {
        // Arrange
        var strategy = new DhlRateStrategy(new FakeDhlClient(), new DhlRateAdapter());
        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "DHL",
            Endpoints =
            [
                new CarrierEndpoint
                {
                    Id = Guid.NewGuid(),
                    Operation = "Rates",
                    Endpoint = "http://localhost:5204/api/dhl/rates"
                }
            ]
        };

        // Act
        var result = await strategy.TryGetRatesAsync(carrier, new RateQueryRequest(5m, 10m, 5m, 5m), CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("DHL", result.Carrier);
        Assert.Single(result.RateOptions);
        Assert.Equal("DHL Economy Select", result.RateOptions[0].ServiceName);
    }

    private sealed class FakeFedExClient : IMockFedExRatesClient
    {
        public Task<MockFedExRateResponse> GetRatesAsync(string endpoint, MockFedExRateRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new MockFedExRateResponse("FedEx", [new MockFedExServiceOption("FedEx Ground", "2026-06-15", 12.34m)]));
        }
    }

    private sealed class FakeUpsClient : IMockUpsRatesClient
    {
        public Task<MockUpsRateResponse> GetRatesAsync(string endpoint, MockUpsRateRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new MockUpsRateResponse("UPS", [new MockUpsServiceOption("UPS Ground", "2026-06-15", 15.20m)]));
        }
    }

    private sealed class FakeDhlClient : IMockDhlRatesClient
    {
        public Task<MockDhlRateResponse> GetRatesAsync(string endpoint, MockDhlRateRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new MockDhlRateResponse("DHL", [new MockDhlServiceOption("DHL Economy Select", "2026-06-16", 11.00m)]));
        }
    }

    private sealed class CapturingUpsClient : IMockUpsRatesClient
    {
        public MockUpsRateRequest? LastRequest { get; private set; }

        public Task<MockUpsRateResponse> GetRatesAsync(string endpoint, MockUpsRateRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new MockUpsRateResponse("UPS", [new MockUpsServiceOption("UPS Ground", "2026-06-15", 15.20m)]));
        }
    }

    private sealed class CapturingDhlClient : IMockDhlRatesClient
    {
        public MockDhlRateRequest? LastRequest { get; private set; }

        public Task<MockDhlRateResponse> GetRatesAsync(string endpoint, MockDhlRateRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new MockDhlRateResponse("DHL", [new MockDhlServiceOption("DHL Economy Select", "2026-06-16", 11.00m)]));
        }
    }
}
