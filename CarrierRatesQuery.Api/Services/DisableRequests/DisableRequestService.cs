using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.Carriers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CarrierRatesQuery.Api.Services.DisableRequests;

public interface IDisableRequestService
{
    Task<IReadOnlyList<DisableRequestResponseDto>> GetByCarrierAsync(Guid carrierId, CancellationToken cancellationToken);
    Task<DisableRequestResponseDto> CreateAsync(Guid carrierId, string requestedBy, string reason, CancellationToken cancellationToken);
    Task<DisableRequestResponseDto> ApproveAsync(Guid disableRequestId, string processedBy, CancellationToken cancellationToken);
    Task<DisableRequestResponseDto> RejectAsync(Guid disableRequestId, string processedBy, CancellationToken cancellationToken);
}

public sealed class DisableRequestService(
    AppDbContext context,
    ICarrierService carrierService,
    IValidator<DisableCarrierRequest> disableValidator) : IDisableRequestService
{
    public async Task<IReadOnlyList<DisableRequestResponseDto>> GetByCarrierAsync(Guid carrierId, CancellationToken cancellationToken)
    {
        var carrierExists = await context.Carriers.AnyAsync(x => x.Id == carrierId, cancellationToken);
        if (!carrierExists)
        {
            throw new CarrierNotFoundException(carrierId);
        }

        var requests = await context.DisableRequests
            .AsNoTracking()
            .Where(x => x.CarrierId == carrierId)
            .OrderByDescending(x => x.RequestedAtUtc)
            .ToListAsync(cancellationToken);

        return requests.Select(Map).ToList();
    }

    public async Task<DisableRequestResponseDto> CreateAsync(
        Guid carrierId,
        string requestedBy,
        string reason,
        CancellationToken cancellationToken)
    {
        await disableValidator.ValidateAndThrowAsync(new DisableCarrierRequest(reason), cancellationToken);

        var carrierExists = await context.Carriers.AnyAsync(x => x.Id == carrierId, cancellationToken);
        if (!carrierExists)
        {
            throw new CarrierNotFoundException(carrierId);
        }

        var entity = new DisableRequest
        {
            Id = Guid.NewGuid(),
            CarrierId = carrierId,
            RequestedBy = requestedBy,
            Reason = reason.Trim(),
            Status = DisableRequestStatus.Pending,
            RequestedAtUtc = DateTime.UtcNow
        };

        context.DisableRequests.Add(entity);
        await context.SaveChangesAsync(cancellationToken);

        return Map(entity);
    }

    public async Task<DisableRequestResponseDto> ApproveAsync(Guid disableRequestId, string processedBy, CancellationToken cancellationToken)
    {
        var request = await context.DisableRequests.SingleOrDefaultAsync(x => x.Id == disableRequestId, cancellationToken);
        if (request is null)
        {
            throw new DisableRequestNotFoundException(disableRequestId);
        }

        if (request.Status != DisableRequestStatus.Pending)
        {
            throw new CarrierConflictException("Only pending disable requests can be approved.");
        }

        await carrierService.DisableAsync(request.CarrierId, new DisableCarrierRequest(request.Reason), cancellationToken);

        request.Status = DisableRequestStatus.Approved;
        request.ProcessedBy = processedBy;
        request.ProcessedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return Map(request);
    }

    public async Task<DisableRequestResponseDto> RejectAsync(Guid disableRequestId, string processedBy, CancellationToken cancellationToken)
    {
        var request = await context.DisableRequests.SingleOrDefaultAsync(x => x.Id == disableRequestId, cancellationToken);
        if (request is null)
        {
            throw new DisableRequestNotFoundException(disableRequestId);
        }

        if (request.Status != DisableRequestStatus.Pending)
        {
            throw new CarrierConflictException("Only pending disable requests can be rejected.");
        }

        request.Status = DisableRequestStatus.Rejected;
        request.ProcessedBy = processedBy;
        request.ProcessedAtUtc = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return Map(request);
    }

    private static DisableRequestResponseDto Map(DisableRequest source)
    {
        return new DisableRequestResponseDto(
            source.Id,
            source.CarrierId,
            source.RequestedBy,
            source.Reason,
            source.Status.ToString(),
            source.RequestedAtUtc,
            source.ProcessedBy,
            source.ProcessedAtUtc);
    }
}

public sealed record DisableRequestResponseDto(
    Guid Id,
    Guid CarrierId,
    string RequestedBy,
    string Reason,
    string Status,
    DateTime RequestedAtUtc,
    string? ProcessedBy,
    DateTime? ProcessedAtUtc);
