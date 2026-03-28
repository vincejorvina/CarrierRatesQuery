using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.Carriers;
using Microsoft.EntityFrameworkCore;

namespace CarrierRatesQuery.Tests.CarrierManagement;

public class CarrierServiceTests
{
    [Fact]
    public async Task DeleteCarrier_CarrierIsEnabled_ThrowsCarrierConflictException()
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
    public async Task DisableCarrier_OnlyActiveCarrier_ThrowsCarrierConflictException()
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
    public async Task DisableCarrier_PendingShipmentExists_ThrowsCarrierConflictException()
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
    public async Task DisableCarrier_PendingSettlementExists_ThrowsCarrierConflictException()
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
    public async Task DisableCarrier_ValidRequest_DisablesCarrierAndWritesAudit()
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
