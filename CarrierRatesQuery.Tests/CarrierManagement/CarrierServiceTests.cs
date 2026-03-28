using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.Carriers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CarrierRatesQuery.Tests.CarrierManagement;

public class CarrierServiceTests
{
    [Fact]
    public async Task GetAllAsync_MultipleCarriers_ReturnsAllOrderedByName()
    {
        // Arrange
        await using var context = CreateDbContext();
        context.Carriers.AddRange(
            new Carrier { Id = Guid.NewGuid(), Name = "UPS", IsEnabled = true, CreatedAtUtc = DateTime.UtcNow },
            new Carrier { Id = Guid.NewGuid(), Name = "FedEx", IsEnabled = true, CreatedAtUtc = DateTime.UtcNow },
            new Carrier { Id = Guid.NewGuid(), Name = "DHL", IsEnabled = false, CreatedAtUtc = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("DHL", result[0].Name);
        Assert.Equal("FedEx", result[1].Name);
        Assert.Equal("UPS", result[2].Name);
    }

    [Fact]
    public async Task GetAllAsync_NoCarriers_ReturnsEmptyList()
    {
        // Arrange
        await using var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.GetAllAsync(CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCarrier_ReturnsCarrierWithEndpoints()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "FedEx",
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow,
            Endpoints =
            [
                new CarrierEndpoint { Id = Guid.NewGuid(), Operation = "Rates", Endpoint = "http://localhost/api/rates" }
            ]
        };
        context.Carriers.Add(carrier);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetByIdAsync(carrier.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(carrier.Id, result.Id);
        Assert.Equal("FedEx", result.Name);
        Assert.Single(result.Endpoints);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentCarrier_ReturnsNull()
    {
        // Arrange
        await using var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesCarrierWithGeneratedId()
    {
        // Arrange
        await using var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.CreateAsync(new CreateCarrierRequest("FedEx", true), CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("FedEx", result.Name);
        Assert.Equal("fedex", result.Slug);
        Assert.True(result.IsEnabled);

        var stored = await context.Carriers.FindAsync(result.Id);
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsValidationException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        Func<Task> action = () => service.CreateAsync(new CreateCarrierRequest("", true), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ValidationException>(action);
    }

    [Fact]
    public async Task UpdateAsync_ExistingCarrier_UpdatesNameAndIsEnabled()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrier = new Carrier { Id = Guid.NewGuid(), Name = "FedEx", IsEnabled = true, CreatedAtUtc = DateTime.UtcNow };
        context.Carriers.Add(carrier);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.UpdateAsync(carrier.Id, new UpdateCarrierRequest("FedEx Express", false), CancellationToken.None);

        // Assert
        Assert.Equal("FedEx Express", result.Name);
        Assert.False(result.IsEnabled);
        Assert.NotNull(result.UpdatedAtUtc);
    }

    [Theory]
    [InlineData("update")]
    [InlineData("enable")]
    [InlineData("disable")]
    [InlineData("delete")]
    public async Task CarrierOperation_NonExistentCarrier_ThrowsCarrierNotFoundException(string operation)
    {
        // Arrange
        await using var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        Func<Task> action = operation switch
        {
            "update" => () => service.UpdateAsync(Guid.NewGuid(), new UpdateCarrierRequest("FedEx", true), CancellationToken.None),
            "enable" => () => service.EnableAsync(Guid.NewGuid(), CancellationToken.None),
            "disable" => () => service.DisableAsync(Guid.NewGuid(), new DisableCarrierRequest("maintenance"), CancellationToken.None),
            "delete" => () => service.DeleteAsync(Guid.NewGuid(), CancellationToken.None),
            _ => throw new InvalidOperationException($"Unknown operation '{operation}'.")
        };

        // Assert
        await Assert.ThrowsAsync<CarrierNotFoundException>(action);
    }

    [Fact]
    public async Task EnableAsync_DisabledCarrier_EnablesCarrier()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrier = new Carrier { Id = Guid.NewGuid(), Name = "FedEx", IsEnabled = false, CreatedAtUtc = DateTime.UtcNow };
        context.Carriers.Add(carrier);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.EnableAsync(carrier.Id, CancellationToken.None);

        // Assert
        Assert.True(result.IsEnabled);
        Assert.NotNull(result.UpdatedAtUtc);
    }

    [Fact]
    public async Task EnableAsync_AlreadyEnabled_ReturnsCarrierWithoutChange()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrier = new Carrier { Id = Guid.NewGuid(), Name = "FedEx", IsEnabled = true, CreatedAtUtc = DateTime.UtcNow };
        context.Carriers.Add(carrier);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.EnableAsync(carrier.Id, CancellationToken.None);

        // Assert
        Assert.True(result.IsEnabled);
        Assert.Null(result.UpdatedAtUtc);
    }

    [Fact]
    public async Task DisableAsync_EmptyReason_ThrowsValidationException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrier = new Carrier { Id = Guid.NewGuid(), Name = "FedEx", IsEnabled = true, CreatedAtUtc = DateTime.UtcNow };
        context.Carriers.Add(carrier);
        var another = new Carrier { Id = Guid.NewGuid(), Name = "UPS", IsEnabled = true, CreatedAtUtc = DateTime.UtcNow };
        context.Carriers.Add(another);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        Func<Task> action = () => service.DisableAsync(carrier.Id, new DisableCarrierRequest(""), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ValidationException>(action);
    }

    [Fact]
    public async Task DeleteAsync_DisabledCarrier_DeletesSuccessfully()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrier = new Carrier { Id = Guid.NewGuid(), Name = "FedEx", IsEnabled = false, CreatedAtUtc = DateTime.UtcNow };
        context.Carriers.Add(carrier);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        await service.DeleteAsync(carrier.Id, CancellationToken.None);

        // Assert
        Assert.Empty(context.Carriers);
    }

    [Fact]
    public async Task DeleteAsync_CarrierIsEnabled_ThrowsCarrierConflictException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "FedEx",
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        context.Carriers.Add(carrier);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var action = () => service.DeleteAsync(carrier.Id, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierConflictException>(action);
    }

    [Fact]
    public async Task DisableAsync_OnlyActiveCarrier_ThrowsCarrierConflictException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "FedEx",
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        context.Carriers.Add(carrier);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var action = () => service.DisableAsync(carrier.Id, new DisableCarrierRequest("maintenance"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierConflictException>(action);
    }

    [Fact]
    public async Task DisableAsync_PendingShipmentExists_ThrowsCarrierConflictException()
    {
        // Arrange
        await using var context = CreateDbContext();

        var target = new Carrier { Id = Guid.NewGuid(), Name = "FedEx", IsEnabled = true, CreatedAtUtc = DateTime.UtcNow };
        var another = new Carrier { Id = Guid.NewGuid(), Name = "UPS", IsEnabled = true, CreatedAtUtc = DateTime.UtcNow };

        context.Carriers.AddRange(target, another);
        context.Shipments.Add(new Shipment
        {
            Id = Guid.NewGuid(),
            CarrierId = target.Id,
            Status = ShipmentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var action = () => service.DisableAsync(target.Id, new DisableCarrierRequest("maintenance"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierConflictException>(action);
    }

    [Fact]
    public async Task DisableAsync_PendingSettlementExists_ThrowsCarrierConflictException()
    {
        // Arrange
        await using var context = CreateDbContext();

        var target = new Carrier { Id = Guid.NewGuid(), Name = "FedEx", IsEnabled = true, CreatedAtUtc = DateTime.UtcNow };
        var another = new Carrier { Id = Guid.NewGuid(), Name = "UPS", IsEnabled = true, CreatedAtUtc = DateTime.UtcNow };

        context.Carriers.AddRange(target, another);
        context.CarrierFinancialSettlements.Add(new CarrierFinancialSettlement
        {
            Id = Guid.NewGuid(),
            CarrierId = target.Id,
            Status = CarrierFinancialSettlementStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var action = () => service.DisableAsync(target.Id, new DisableCarrierRequest("maintenance"), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierConflictException>(action);
    }

    [Fact]
    public async Task DisableAsync_ValidRequest_DisablesCarrierAndWritesAudit()
    {
        // Arrange
        await using var context = CreateDbContext();

        var target = new Carrier
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
        };

        var another = new Carrier { Id = Guid.NewGuid(), Name = "UPS", IsEnabled = true, CreatedAtUtc = DateTime.UtcNow };

        context.Carriers.AddRange(target, another);
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.DisableAsync(target.Id, new DisableCarrierRequest("maintenance"), CancellationToken.None);

        // Assert
        Assert.False(result.IsEnabled);

        var audit = await context.CarrierDisableAudits.SingleAsync(x => x.CarrierId == target.Id);
        Assert.Equal("maintenance", audit.Reason);
    }

    private static CarrierService CreateService(AppDbContext context)
    {
        return new CarrierService(
            context,
            new CreateCarrierRequestValidator(),
            new UpdateCarrierRequestValidator(),
            new DisableCarrierRequestValidator());
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"carrier-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }
}
