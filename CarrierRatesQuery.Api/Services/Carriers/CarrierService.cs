using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CarrierRatesQuery.Api.Services.Carriers;

public interface ICarrierService
{
    Task<IReadOnlyList<CarrierResponseDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<CarrierResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CarrierResponseDto> CreateAsync(CreateCarrierRequest request, CancellationToken cancellationToken);
    Task<CarrierResponseDto> UpdateAsync(Guid id, UpdateCarrierRequest request, CancellationToken cancellationToken);
    Task<CarrierResponseDto> EnableAsync(Guid id, CancellationToken cancellationToken);
    Task<CarrierResponseDto> DisableAsync(Guid id, DisableCarrierRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public sealed class CarrierService(
    AppDbContext context,
    IValidator<CreateCarrierRequest> createValidator,
    IValidator<UpdateCarrierRequest> updateValidator,
    IValidator<DisableCarrierRequest> disableValidator) : ICarrierService
{
    public async Task<IReadOnlyList<CarrierResponseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var carriers = await context.Carriers
            .Include(x => x.Endpoints)
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return carriers.Select(MapToResponse).ToList();
    }

    public async Task<CarrierResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var carrier = await context.Carriers
            .Include(x => x.Endpoints)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return carrier is null ? null : MapToResponse(carrier);
    }

    public async Task<CarrierResponseDto> CreateAsync(CreateCarrierRequest request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var now = DateTime.UtcNow;
        var carrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            IsEnabled = request.IsEnabled,
            CreatedAtUtc = now
        };

        context.Carriers.Add(carrier);
        await context.SaveChangesAsync(cancellationToken);

        return MapToResponse(carrier);
    }

    public async Task<CarrierResponseDto> UpdateAsync(Guid id, UpdateCarrierRequest request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var carrier = await context.Carriers
            .Include(x => x.Endpoints)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (carrier is null)
        {
            throw new CarrierNotFoundException(id);
        }

        carrier.Name = request.Name.Trim();
        carrier.IsEnabled = request.IsEnabled;
        carrier.UpdatedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return MapToResponse(carrier);
    }

    public async Task<CarrierResponseDto> EnableAsync(Guid id, CancellationToken cancellationToken)
    {
        var carrier = await context.Carriers
            .Include(x => x.Endpoints)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (carrier is null)
        {
            throw new CarrierNotFoundException(id);
        }

        if (!carrier.IsEnabled)
        {
            carrier.IsEnabled = true;
            carrier.UpdatedAtUtc = DateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        return MapToResponse(carrier);
    }

    public async Task<CarrierResponseDto> DisableAsync(Guid id, DisableCarrierRequest request, CancellationToken cancellationToken)
    {
        await disableValidator.ValidateAndThrowAsync(request, cancellationToken);

        var carrier = await context.Carriers
            .Include(x => x.Endpoints)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (carrier is null)
        {
            throw new CarrierNotFoundException(id);
        }

        if (carrier.IsEnabled)
        {
            var enabledCarrierCount = await context.Carriers
                .CountAsync(x => x.IsEnabled, cancellationToken);

            if (enabledCarrierCount <= 1)
            {
                throw new CarrierConflictException("A carrier cannot be disabled when it is the only active carrier.");
            }

            var hasPendingShipments = await context.Shipments
                .AnyAsync(x => x.CarrierId == id && x.Status == ShipmentStatus.Pending, cancellationToken);

            if (hasPendingShipments)
            {
                throw new CarrierConflictException("A carrier with ongoing shipments cannot be disabled.");
            }

            var hasPendingSettlements = await context.CarrierFinancialSettlements
                .AnyAsync(x => x.CarrierId == id && x.Status == CarrierFinancialSettlementStatus.Pending, cancellationToken);

            if (hasPendingSettlements)
            {
                throw new CarrierConflictException("A carrier with pending invoices or settlements cannot be disabled.");
            }

            carrier.IsEnabled = false;
            carrier.UpdatedAtUtc = DateTime.UtcNow;

            context.CarrierDisableAudits.Add(new CarrierDisableAudit
            {
                Id = Guid.NewGuid(),
                CarrierId = carrier.Id,
                Reason = request.Reason.Trim(),
                DisabledAtUtc = DateTime.UtcNow
            });

            await context.SaveChangesAsync(cancellationToken);
        }

        return MapToResponse(carrier);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var carrier = await context.Carriers.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (carrier is null)
        {
            throw new CarrierNotFoundException(id);
        }

        if (carrier.IsEnabled)
        {
            throw new CarrierConflictException("Carrier must be disabled before it can be deleted.");
        }

        context.Carriers.Remove(carrier);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static CarrierResponseDto MapToResponse(Carrier carrier)
    {
        return new CarrierResponseDto(
            Id: carrier.Id,
            Name: carrier.Name,
            Slug: carrier.Slug,
            IsEnabled: carrier.IsEnabled,
            CreatedAtUtc: carrier.CreatedAtUtc,
            UpdatedAtUtc: carrier.UpdatedAtUtc,
            Endpoints: carrier.Endpoints
                .OrderBy(x => x.Operation)
                .Select(endpoint => new CarrierEndpointDto(
                    Id: endpoint.Id,
                    Operation: endpoint.Operation,
                    Endpoint: endpoint.Endpoint
                ))
                .ToList()
        );
    }
}
