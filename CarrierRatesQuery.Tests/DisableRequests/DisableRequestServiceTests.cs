using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.Carriers;
using CarrierRatesQuery.Api.Services.DisableRequests;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CarrierRatesQuery.Tests.DisableRequests;

public class DisableRequestServiceTests
{
    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesPendingDisableRequest()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var service = CreateService(context, new FakeCarrierService());

        // Act
        var result = await service.CreateAsync(carrierId, "user.demo", "maintenance", CancellationToken.None);

        // Assert
        Assert.Equal(carrierId, result.CarrierId);
        Assert.Equal("Pending", result.Status);
        Assert.Equal("user.demo", result.RequestedBy);
        Assert.Equal("maintenance", result.Reason);
    }

    [Fact]
    public async Task CreateAsync_ReasonIsEmpty_ThrowsValidationException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var service = CreateService(context, new FakeCarrierService());

        // Act
        Func<Task> action = () => service.CreateAsync(carrierId, "user.demo", string.Empty, CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ValidationException>(action);
    }

    [Fact]
    public async Task GetByCarrierAsync_MultipleRequests_ReturnsByNewestFirst()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");

        context.DisableRequests.AddRange(
            new DisableRequest
            {
                Id = Guid.NewGuid(),
                CarrierId = carrierId,
                RequestedBy = "user.one",
                Reason = "maintenance",
                Status = DisableRequestStatus.Pending,
                RequestedAtUtc = DateTime.UtcNow.AddMinutes(-5)
            },
            new DisableRequest
            {
                Id = Guid.NewGuid(),
                CarrierId = carrierId,
                RequestedBy = "user.two",
                Reason = "user request",
                Status = DisableRequestStatus.Pending,
                RequestedAtUtc = DateTime.UtcNow
            });
        await context.SaveChangesAsync();

        var service = CreateService(context, new FakeCarrierService());

        // Act
        var result = await service.GetByCarrierAsync(carrierId, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("user.two", result[0].RequestedBy);
        Assert.Equal("user.one", result[1].RequestedBy);
    }

    [Fact]
    public async Task ApproveAsync_PendingRequest_DisablesCarrierAndMarksApproved()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var requestId = Guid.NewGuid();

        context.DisableRequests.Add(new DisableRequest
        {
            Id = requestId,
            CarrierId = carrierId,
            RequestedBy = "user.demo",
            Reason = "maintenance",
            Status = DisableRequestStatus.Pending,
            RequestedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var fakeCarrierService = new FakeCarrierService();
        var service = CreateService(context, fakeCarrierService);

        // Act
        var result = await service.ApproveAsync(requestId, "admin.demo", CancellationToken.None);

        // Assert
        Assert.Equal("Approved", result.Status);
        Assert.Equal("admin.demo", result.ProcessedBy);
        Assert.Equal(carrierId, fakeCarrierService.DisabledCarrierId);
        Assert.Equal("maintenance", fakeCarrierService.DisableReason);
    }

    [Fact]
    public async Task RejectAsync_PendingRequest_MarksRequestRejected()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var requestId = Guid.NewGuid();

        context.DisableRequests.Add(new DisableRequest
        {
            Id = requestId,
            CarrierId = carrierId,
            RequestedBy = "user.demo",
            Reason = "maintenance",
            Status = DisableRequestStatus.Pending,
            RequestedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateService(context, new FakeCarrierService());

        // Act
        var result = await service.RejectAsync(requestId, "admin.demo", CancellationToken.None);

        // Assert
        Assert.Equal("Rejected", result.Status);
        Assert.Equal("admin.demo", result.ProcessedBy);
    }

    [Fact]
    public async Task ApproveAsync_AlreadyProcessedRequest_ThrowsCarrierConflictException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var requestId = Guid.NewGuid();

        context.DisableRequests.Add(new DisableRequest
        {
            Id = requestId,
            CarrierId = carrierId,
            RequestedBy = "user.demo",
            Reason = "maintenance",
            Status = DisableRequestStatus.Rejected,
            RequestedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateService(context, new FakeCarrierService());

        // Act
        Func<Task> action = () => service.ApproveAsync(requestId, "admin.demo", CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierConflictException>(action);
    }

    [Fact]
    public async Task CreateAsync_CarrierDoesNotExist_ThrowsCarrierNotFoundException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var service = CreateService(context, new FakeCarrierService());

        // Act
        Func<Task> action = () => service.CreateAsync(Guid.NewGuid(), "user.demo", "maintenance", CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierNotFoundException>(action);
    }

    [Fact]
    public async Task GetByCarrierAsync_CarrierDoesNotExist_ThrowsCarrierNotFoundException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var service = CreateService(context, new FakeCarrierService());

        // Act
        Func<Task> action = () => service.GetByCarrierAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierNotFoundException>(action);
    }

    [Fact]
    public async Task ApproveAsync_RequestDoesNotExist_ThrowsDisableRequestNotFoundException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var service = CreateService(context, new FakeCarrierService());

        // Act
        Func<Task> action = () => service.ApproveAsync(Guid.NewGuid(), "admin.demo", CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<DisableRequestNotFoundException>(action);
    }

    [Fact]
    public async Task RejectAsync_AlreadyProcessedRequest_ThrowsCarrierConflictException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var requestId = Guid.NewGuid();

        context.DisableRequests.Add(new DisableRequest
        {
            Id = requestId,
            CarrierId = carrierId,
            RequestedBy = "user.demo",
            Reason = "maintenance",
            Status = DisableRequestStatus.Approved,
            RequestedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateService(context, new FakeCarrierService());

        // Act
        Func<Task> action = () => service.RejectAsync(requestId, "admin.demo", CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<CarrierConflictException>(action);
    }

    [Fact]
    public async Task RejectAsync_RequestDoesNotExist_ThrowsDisableRequestNotFoundException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var service = CreateService(context, new FakeCarrierService());

        // Act
        Func<Task> action = () => service.RejectAsync(Guid.NewGuid(), "admin.demo", CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<DisableRequestNotFoundException>(action);
    }

    [Fact]
    public async Task GetByCarrierAsync_CarrierHasNoRequests_ReturnsEmptyList()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var service = CreateService(context, new FakeCarrierService());

        // Act
        var result = await service.GetByCarrierAsync(carrierId, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task ApproveAsync_PendingRequest_SetsProcessedAtUtc()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var requestId = Guid.NewGuid();

        context.DisableRequests.Add(new DisableRequest
        {
            Id = requestId,
            CarrierId = carrierId,
            RequestedBy = "user.demo",
            Reason = "maintenance",
            Status = DisableRequestStatus.Pending,
            RequestedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = CreateService(context, new FakeCarrierService());

        // Act
        var result = await service.ApproveAsync(requestId, "admin.demo", CancellationToken.None);

        // Assert
        Assert.NotNull(result.ProcessedAtUtc);
    }

    [Fact]
    public async Task ApproveAsync_OnlyActiveCarrier_PropagatesCarrierConflictException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var requestId = Guid.NewGuid();

        context.DisableRequests.Add(new DisableRequest
        {
            Id = requestId,
            CarrierId = carrierId,
            RequestedBy = "user.demo",
            Reason = "maintenance",
            Status = DisableRequestStatus.Pending,
            RequestedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var failingCarrierService = new FailingCarrierService(
            new CarrierConflictException("Cannot disable the only active carrier."));
        var service = CreateService(context, failingCarrierService);

        // Act
        Func<Task> action = () => service.ApproveAsync(requestId, "admin.demo", CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<CarrierConflictException>(action);
        Assert.Contains("only active carrier", ex.Message);
    }

    [Fact]
    public async Task ApproveAsync_PendingShipments_PropagatesCarrierConflictException()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var requestId = Guid.NewGuid();

        context.DisableRequests.Add(new DisableRequest
        {
            Id = requestId,
            CarrierId = carrierId,
            RequestedBy = "user.demo",
            Reason = "maintenance",
            Status = DisableRequestStatus.Pending,
            RequestedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var failingCarrierService = new FailingCarrierService(
            new CarrierConflictException("Carrier has pending shipments and cannot be disabled."));
        var service = CreateService(context, failingCarrierService);

        // Act
        Func<Task> action = () => service.ApproveAsync(requestId, "admin.demo", CancellationToken.None);

        // Assert
        var ex = await Assert.ThrowsAsync<CarrierConflictException>(action);
        Assert.Contains("pending shipments", ex.Message);
    }

    [Fact]
    public async Task ApproveAsync_CarrierDisableFails_RequestRemainsPending()
    {
        // Arrange
        await using var context = CreateDbContext();
        var carrierId = await AddCarrierAsync(context, "FedEx");
        var requestId = Guid.NewGuid();

        context.DisableRequests.Add(new DisableRequest
        {
            Id = requestId,
            CarrierId = carrierId,
            RequestedBy = "user.demo",
            Reason = "maintenance",
            Status = DisableRequestStatus.Pending,
            RequestedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var failingCarrierService = new FailingCarrierService(
            new CarrierConflictException("Cannot disable the only active carrier."));
        var service = CreateService(context, failingCarrierService);

        // Act
        try
        {
            await service.ApproveAsync(requestId, "admin.demo", CancellationToken.None);
        }
        catch (CarrierConflictException)
        {
            // Expected
        }

        // Assert - request should still be pending since the operation failed
        var request = await context.DisableRequests.SingleAsync(x => x.Id == requestId);
        Assert.Equal(DisableRequestStatus.Pending, request.Status);
    }

    private static DisableRequestService CreateService(AppDbContext context, ICarrierService carrierService)
    {
        return new DisableRequestService(context, carrierService, new DisableCarrierRequestValidator());
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
            .UseInMemoryDatabase($"disable-request-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private sealed class FakeCarrierService : ICarrierService
    {
        public Guid? DisabledCarrierId { get; private set; }
        public string? DisableReason { get; private set; }

        public Task<IReadOnlyList<CarrierResponseDto>> GetAllAsync(CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<CarrierResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<CarrierResponseDto> CreateAsync(CreateCarrierRequest request, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<CarrierResponseDto> UpdateAsync(Guid id, UpdateCarrierRequest request, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<CarrierResponseDto> EnableAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<CarrierResponseDto> DisableAsync(Guid id, DisableCarrierRequest request, CancellationToken cancellationToken)
        {
            DisabledCarrierId = id;
            DisableReason = request.Reason;

            return Task.FromResult(new CarrierResponseDto(
                id,
                "FedEx",
                "fedex",
                false,
                DateTime.UtcNow,
                DateTime.UtcNow,
                []));
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }

    private sealed class FailingCarrierService(Exception exceptionToThrow) : ICarrierService
    {
        public Task<IReadOnlyList<CarrierResponseDto>> GetAllAsync(CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<CarrierResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<CarrierResponseDto> CreateAsync(CreateCarrierRequest request, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<CarrierResponseDto> UpdateAsync(Guid id, UpdateCarrierRequest request, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<CarrierResponseDto> EnableAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotImplementedException();

        public Task<CarrierResponseDto> DisableAsync(Guid id, DisableCarrierRequest request, CancellationToken cancellationToken) =>
            throw exceptionToThrow;

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }
}
