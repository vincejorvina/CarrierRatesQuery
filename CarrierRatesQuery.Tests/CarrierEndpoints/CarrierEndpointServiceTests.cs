using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.CarrierEndpoints;
using Microsoft.EntityFrameworkCore;

namespace CarrierRatesQuery.Tests.CarrierEndpoints;

public class CarrierEndpointServiceTests
{
    [Fact]
    public async Task CreateEndpoint_ValidRequest_CreatesEndpointForCarrier()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var service = CreateService(context);

        // Act
        var response = await service.CreateAsync(
            carrierId,
            new CreateCarrierEndpointRequest("Rates", "http://localhost:5133/api/fedex/rates"),
            CancellationToken.None);

        // Assert
        Assert.Equal(carrierId, response.CarrierId);
        Assert.Equal("Rates", response.Operation);
        Assert.Equal("http://localhost:5133/api/fedex/rates", response.Endpoint);
    }

    [Fact]
    public async Task UpdateEndpoint_ValidRequest_UpdatesEndpointValues()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var endpointId = Guid.NewGuid();

        context.CarrierEndpoints.Add(new CarrierEndpoint
        {
            Id = endpointId,
            CarrierId = carrierId,
            Operation = "Rates",
            Endpoint = "http://localhost:5133/api/fedex/rates"
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var response = await service.UpdateAsync(
            carrierId,
            endpointId,
            new UpdateCarrierEndpointRequest("Tracking", "http://localhost:5133/api/fedex/tracking"),
            CancellationToken.None);

        // Assert
        Assert.Equal("Tracking", response.Operation);
        Assert.Equal("http://localhost:5133/api/fedex/tracking", response.Endpoint);
    }

    [Fact]
    public async Task DeleteEndpoint_ExistingEndpoint_RemovesEndpoint()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var endpointId = Guid.NewGuid();

        context.CarrierEndpoints.Add(new CarrierEndpoint
        {
            Id = endpointId,
            CarrierId = carrierId,
            Operation = "Rates",
            Endpoint = "http://localhost:5133/api/fedex/rates"
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        await service.DeleteAsync(carrierId, endpointId, CancellationToken.None);

        // Assert
        var exists = await context.CarrierEndpoints.AnyAsync(x => x.Id == endpointId);
        Assert.False(exists);
    }

    private static CarrierEndpointService CreateService(AppDbContext context)
    {
        return new CarrierEndpointService(
            context,
            new CreateCarrierEndpointRequestValidator(),
            new UpdateCarrierEndpointRequestValidator());
    }

    private static async Task<Guid> AddCarrierAsync(AppDbContext context, string name)
    {
        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = name,
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        context.Carriers.Add(carrier);
        await context.SaveChangesAsync();
        return carrier.Id;
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"endpoint-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }
}
