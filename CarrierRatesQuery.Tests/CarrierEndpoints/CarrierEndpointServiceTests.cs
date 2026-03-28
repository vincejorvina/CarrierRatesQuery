using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.CarrierEndpoints;
using CarrierRatesQuery.Api.Services.Carriers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CarrierRatesQuery.Tests.CarrierEndpoints;

public class CarrierEndpointServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesEndpointForCarrier()
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
    public async Task UpdateAsync_ValidRequest_UpdatesEndpointValues()
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
    public async Task DeleteAsync_ExistingEndpoint_RemovesEndpoint()
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

    [Theory]
    [InlineData("", "http://localhost:5133/api/fedex/rates")]
    [InlineData("Rates", "")]
    public async Task CreateAsync_InvalidRequest_ThrowsValidationException(string operation, string endpointUrl)
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var service = CreateService(context);

        // Act
        Func<Task> action = () => service.CreateAsync(
            carrierId,
            new CreateCarrierEndpointRequest(operation, endpointUrl),
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ValidationException>(action);
    }

    [Fact]
    public async Task GetAllAsync_CarrierDoesNotExist_ThrowsCarrierNotFoundException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        Func<Task> action = () => service.GetAllAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierNotFoundException>(action);
    }

    [Fact]
    public async Task DeleteAsync_EndpointDoesNotExist_ThrowsCarrierEndpointNotFoundException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var service = CreateService(context);

        // Act
        Func<Task> action = () => service.DeleteAsync(carrierId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierEndpointNotFoundException>(action);
    }

    [Fact]
    public async Task GetAllAsync_CarrierWithMultipleEndpoints_ReturnsAllOrderedByOperation()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");

        context.CarrierEndpoints.AddRange(
            new CarrierEndpoint { Id = Guid.NewGuid(), CarrierId = carrierId, Operation = "Tracking", Endpoint = "http://localhost/api/tracking" },
            new CarrierEndpoint { Id = Guid.NewGuid(), CarrierId = carrierId, Operation = "Rates", Endpoint = "http://localhost/api/rates" });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetAllAsync(carrierId, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Rates", result[0].Operation);
        Assert.Equal("Tracking", result[1].Operation);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEndpoint_ReturnsEndpoint()
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
            Endpoint = "http://localhost/api/rates"
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetByIdAsync(carrierId, endpointId, CancellationToken.None);

        // Assert
        Assert.Equal(endpointId, result.Id);
        Assert.Equal(carrierId, result.CarrierId);
        Assert.Equal("Rates", result.Operation);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentEndpoint_ThrowsCarrierEndpointNotFoundException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var service = CreateService(context);

        // Act
        Func<Task> action = () => service.GetByIdAsync(carrierId, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierEndpointNotFoundException>(action);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEndpoint_ThrowsCarrierEndpointNotFoundException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var service = CreateService(context);

        // Act
        Func<Task> action = () => service.UpdateAsync(
            carrierId,
            Guid.NewGuid(),
            new UpdateCarrierEndpointRequest("Tracking", "http://localhost/api/tracking"),
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierEndpointNotFoundException>(action);
    }

    [Theory]
    [InlineData("", "http://localhost/api/rates")]
    [InlineData("Rates", "")]
    public async Task UpdateAsync_InvalidRequest_ThrowsValidationException(string operation, string endpointUrl)
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
            Endpoint = "http://localhost/api/rates"
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        Func<Task> action = () => service.UpdateAsync(
            carrierId,
            endpointId,
            new UpdateCarrierEndpointRequest(operation, endpointUrl),
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ValidationException>(action);
    }

    [Fact]
    public async Task GetByIdAsync_CarrierDoesNotExist_ThrowsCarrierNotFoundException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        Func<Task> action = () => service.GetByIdAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierNotFoundException>(action);
    }

    [Fact]
    public async Task CreateAsync_CarrierDoesNotExist_ThrowsCarrierNotFoundException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        Func<Task> action = () => service.CreateAsync(
            Guid.NewGuid(),
            new CreateCarrierEndpointRequest("Rates", "http://localhost/api/rates"),
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierNotFoundException>(action);
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
