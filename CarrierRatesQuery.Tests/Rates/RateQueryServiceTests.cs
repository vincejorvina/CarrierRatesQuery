using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.Carriers;
using CarrierRatesQuery.Api.Services.Rates;
using CarrierRatesQuery.Api.Services.Rates.Adapters;
using CarrierRatesQuery.Api.Services.Rates.Clients;
using CarrierRatesQuery.Api.Services.Rates.Strategies;
using Microsoft.EntityFrameworkCore;

namespace CarrierRatesQuery.Tests.Rates;

public class RateQueryServiceTests
{
    [Fact]
    public async Task QueryAllEnabledCarriers_FedExAndUpsEnabled_ReturnsBothCarrierRates()
    {
        // Arrange
        await using var context = CreateDbContext();
        context.Carriers.AddRange(
            new Carrier
            {
                Id = Guid.NewGuid(),
                Name = "FedEx",
                IsEnabled = true,
                CreatedAtUtc = DateTime.UtcNow,
                Endpoints =
                [
                    new CarrierEndpoint
                    {
                        Id = Guid.NewGuid(),
                        Operation = "Rates",
                        Endpoint = "http://localhost:5133/api/fedex/rates"
                    }
                ]
            },
            new Carrier
            {
                Id = Guid.NewGuid(),
                Name = "UPS",
                IsEnabled = true,
                CreatedAtUtc = DateTime.UtcNow,
                Endpoints =
                [
                    new CarrierEndpoint
                    {
                        Id = Guid.NewGuid(),
                        Operation = "Rates",
                        Endpoint = "http://localhost:5200/api/ups/rates"
                    }
                ]
            });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.QueryAllEnabledCarriersAsync(new RateQueryRequest(5m, 10m, 5m, 5m), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Carrier == "FedEx");
        Assert.Contains(result, x => x.Carrier == "UPS");
    }

    [Fact]
    public async Task QueryCarrier_DisabledCarrier_ThrowsCarrierConflictException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "FedEx",
            IsEnabled = false,
            CreatedAtUtc = DateTime.UtcNow,
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

        context.Carriers.Add(carrier);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var action = () => service.QueryCarrierAsync(carrier.Id, new RateQueryRequest(5m, 10m, 5m, 5m), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierConflictException>(action);
    }

    [Fact]
    public async Task QueryCarrierBySlug_ValidSlug_ReturnsCarrierRate()
    {
        // Arrange
        await using var context = CreateDbContext();
        context.Carriers.Add(new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "FedEx",
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow,
            Endpoints =
            [
                new CarrierEndpoint
                {
                    Id = Guid.NewGuid(),
                    Operation = "Rates",
                    Endpoint = "http://localhost:5133/api/fedex/rates"
                }
            ]
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.QueryCarrierBySlugAsync("FeDeX", new RateQueryRequest(5m, 10m, 5m, 5m), CancellationToken.None);

        // Assert
        Assert.Equal("FedEx", result.Carrier);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"rate-query-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private static RateQueryService CreateService(AppDbContext context)
    {
        var fedExAdapter = new FedExRateAdapter();
        var upsAdapter = new UpsRateAdapter();
        var dhlAdapter = new DhlRateAdapter();

        var strategies = new ICarrierRateStrategy[]
        {
            new FedExRateStrategy(new FakeFedExClient(), fedExAdapter),
            new UpsRateStrategy(new FakeUpsClient(), upsAdapter),
            new DhlRateStrategy(new FakeDhlClient(), dhlAdapter)
        };

        return new RateQueryService(
            context,
            new RateQueryRequestValidator(),
            new CarrierRateStrategyResolver(strategies));
    }

    private sealed class FakeFedExClient : IMockFedExRatesClient
    {
        public Task<MockFedExRateResponse> GetRatesAsync(
            string endpoint,
            MockFedExRateRequest request,
            CancellationToken cancellationToken)
        {
            var response = new MockFedExRateResponse(
                Carrier: "FedEx",
                ServiceOptions:
                [
                    new MockFedExServiceOption(
                        ServiceName: "FedEx Ground",
                        EstimatedDelivery: "2026-06-15",
                        Rate: 12.34m)
                ]);

            return Task.FromResult(response);
        }
    }

    private sealed class FakeUpsClient : IMockUpsRatesClient
    {
        public Task<MockUpsRateResponse> GetRatesAsync(
            string endpoint,
            MockUpsRateRequest request,
            CancellationToken cancellationToken)
        {
            var response = new MockUpsRateResponse(
                Company: "UPS",
                Services:
                [
                    new MockUpsServiceOption(
                        Service: "UPS Ground",
                        Eta: "2026-06-15",
                        Cost: 15.20m)
                ]);

            return Task.FromResult(response);
        }
    }

    private sealed class FakeDhlClient : IMockDhlRatesClient
    {
        public Task<MockDhlRateResponse> GetRatesAsync(
            string endpoint,
            MockDhlRateRequest request,
            CancellationToken cancellationToken)
        {
            var response = new MockDhlRateResponse(
                Provider: "DHL",
                Options:
                [
                    new MockDhlServiceOption(
                        Name: "DHL Economy Select",
                        DeliveryDate: "2026-06-16",
                        Price: 11.00m)
                ]);

            return Task.FromResult(response);
        }
    }
}
