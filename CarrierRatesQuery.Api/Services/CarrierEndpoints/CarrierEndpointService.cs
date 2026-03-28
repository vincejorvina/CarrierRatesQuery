using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.Carriers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CarrierRatesQuery.Api.Services.CarrierEndpoints;

public interface ICarrierEndpointService
{
    Task<IReadOnlyList<CarrierEndpointResponseDto>> GetAllAsync(Guid carrierId, CancellationToken cancellationToken);
    Task<CarrierEndpointResponseDto> GetByIdAsync(Guid carrierId, Guid endpointId, CancellationToken cancellationToken);
    Task<CarrierEndpointResponseDto> CreateAsync(Guid carrierId, CreateCarrierEndpointRequest request, CancellationToken cancellationToken);
    Task<CarrierEndpointResponseDto> UpdateAsync(Guid carrierId, Guid endpointId, UpdateCarrierEndpointRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid carrierId, Guid endpointId, CancellationToken cancellationToken);
}

public sealed class CarrierEndpointService(
    AppDbContext context,
    IValidator<CreateCarrierEndpointRequest> createValidator,
    IValidator<UpdateCarrierEndpointRequest> updateValidator) : ICarrierEndpointService
{
    public async Task<IReadOnlyList<CarrierEndpointResponseDto>> GetAllAsync(Guid carrierId, CancellationToken cancellationToken)
    {
        await EnsureCarrierExists(carrierId, cancellationToken);

        var endpoints = await context.CarrierEndpoints
            .AsNoTracking()
            .Where(x => x.CarrierId == carrierId)
            .OrderBy(x => x.Operation)
            .ToListAsync(cancellationToken);

        return endpoints.Select(MapToResponse).ToList();
    }

    public async Task<CarrierEndpointResponseDto> GetByIdAsync(Guid carrierId, Guid endpointId, CancellationToken cancellationToken)
    {
        await EnsureCarrierExists(carrierId, cancellationToken);

        var endpoint = await GetEndpointEntity(carrierId, endpointId, cancellationToken);
        return MapToResponse(endpoint);
    }

    public async Task<CarrierEndpointResponseDto> CreateAsync(Guid carrierId, CreateCarrierEndpointRequest request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);
        await EnsureCarrierExists(carrierId, cancellationToken);

        var endpoint = new CarrierEndpoint
        {
            Id = Guid.NewGuid(),
            CarrierId = carrierId,
            Operation = request.Operation.Trim(),
            Endpoint = request.Endpoint.Trim()
        };

        context.CarrierEndpoints.Add(endpoint);
        await context.SaveChangesAsync(cancellationToken);

        return MapToResponse(endpoint);
    }

    public async Task<CarrierEndpointResponseDto> UpdateAsync(Guid carrierId, Guid endpointId, UpdateCarrierEndpointRequest request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);
        await EnsureCarrierExists(carrierId, cancellationToken);

        var endpoint = await GetEndpointEntity(carrierId, endpointId, cancellationToken);
        endpoint.Operation = request.Operation.Trim();
        endpoint.Endpoint = request.Endpoint.Trim();

        await context.SaveChangesAsync(cancellationToken);

        return MapToResponse(endpoint);
    }

    public async Task DeleteAsync(Guid carrierId, Guid endpointId, CancellationToken cancellationToken)
    {
        await EnsureCarrierExists(carrierId, cancellationToken);

        var endpoint = await GetEndpointEntity(carrierId, endpointId, cancellationToken);
        context.CarrierEndpoints.Remove(endpoint);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureCarrierExists(Guid carrierId, CancellationToken cancellationToken)
    {
        var carrierExists = await context.Carriers.AnyAsync(x => x.Id == carrierId, cancellationToken);
        if (!carrierExists)
        {
            throw new CarrierNotFoundException(carrierId);
        }
    }

    private async Task<CarrierEndpoint> GetEndpointEntity(Guid carrierId, Guid endpointId, CancellationToken cancellationToken)
    {
        var endpoint = await context.CarrierEndpoints
            .SingleOrDefaultAsync(x => x.Id == endpointId && x.CarrierId == carrierId, cancellationToken);

        if (endpoint is null)
        {
            throw new CarrierEndpointNotFoundException(carrierId, endpointId);
        }

        return endpoint;
    }

    private static CarrierEndpointResponseDto MapToResponse(CarrierEndpoint endpoint)
    {
        return new CarrierEndpointResponseDto(
            Id: endpoint.Id,
            CarrierId: endpoint.CarrierId,
            Operation: endpoint.Operation,
            Endpoint: endpoint.Endpoint
        );
    }
}
