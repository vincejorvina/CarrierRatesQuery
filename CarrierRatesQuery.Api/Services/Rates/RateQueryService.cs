using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.Carriers;
using CarrierRatesQuery.Api.Services.Rates.Strategies;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;

namespace CarrierRatesQuery.Api.Services.Rates;

public interface IRateQueryService
{
    Task<IReadOnlyList<ShippingRateResponseDto>> QueryAllEnabledCarriersAsync(RateQueryRequest request, CancellationToken cancellationToken);
    Task<ShippingRateResponseDto> QueryCarrierAsync(Guid carrierId, RateQueryRequest request, CancellationToken cancellationToken);
    Task<ShippingRateResponseDto> QueryCarrierBySlugAsync(string carrierSlug, RateQueryRequest request, CancellationToken cancellationToken);
}

public sealed class RateQueryService(
    AppDbContext context,
    IValidator<RateQueryRequest> validator,
    ICarrierRateStrategyResolver strategyResolver,
    IMemoryCache memoryCache) : IRateQueryService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    public async Task<IReadOnlyList<ShippingRateResponseDto>> QueryAllEnabledCarriersAsync(
        RateQueryRequest request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var carriers = await context.Carriers
            .Include(x => x.Endpoints)
            .Where(x => x.IsEnabled)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var responses = new List<ShippingRateResponseDto>();

        foreach (var carrier in carriers)
        {
            var response = await TryQueryCarrierWithCacheAsync(carrier, request, cancellationToken);
            if (response is not null)
            {
                responses.Add(response);
            }
        }

        return responses;
    }

    public async Task<ShippingRateResponseDto> QueryCarrierAsync(
        Guid carrierId,
        RateQueryRequest request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var carrier = await context.Carriers
            .Include(x => x.Endpoints)
            .SingleOrDefaultAsync(x => x.Id == carrierId, cancellationToken);

        if (carrier is null)
        {
            throw new CarrierNotFoundException(carrierId);
        }

        if (!carrier.IsEnabled)
        {
            throw new CarrierConflictException("Carrier is disabled and cannot be queried.");
        }

        return await QueryCarrierInternalAsync(carrier, request, cancellationToken);
    }

    public async Task<ShippingRateResponseDto> QueryCarrierBySlugAsync(
        string carrierSlug,
        RateQueryRequest request,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        if (string.IsNullOrWhiteSpace(carrierSlug))
        {
            throw new CarrierSlugNotFoundException(carrierSlug);
        }

        var normalizedSlug = carrierSlug.Trim().ToLowerInvariant();

        var carriers = await context.Carriers
            .Include(x => x.Endpoints)
            .ToListAsync(cancellationToken);

        var carrier = carriers.SingleOrDefault(x => x.Slug == normalizedSlug);
        if (carrier is null)
        {
            throw new CarrierSlugNotFoundException(carrierSlug);
        }

        if (!carrier.IsEnabled)
        {
            throw new CarrierConflictException("Carrier is disabled and cannot be queried.");
        }

        return await QueryCarrierInternalAsync(carrier, request, cancellationToken);
    }

    private async Task<ShippingRateResponseDto?> TryQueryCarrierAsync(
        Carrier carrier,
        RateQueryRequest request,
        CancellationToken cancellationToken)
    {
        if (!strategyResolver.TryResolve(carrier.Slug, out var strategy))
        {
            return null;
        }

        return await strategy.TryGetRatesAsync(carrier, request, cancellationToken);
    }

    private async Task<ShippingRateResponseDto?> TryQueryCarrierWithCacheAsync(
        Carrier carrier,
        RateQueryRequest request,
        CancellationToken cancellationToken)
    {
        var cacheKey = BuildCarrierRateCacheKey(carrier, request);

        if (memoryCache.TryGetValue(cacheKey, out ShippingRateResponseDto? cached) && cached is not null)
        {
            return cached;
        }

        var response = await TryQueryCarrierAsync(carrier, request, cancellationToken);
        if (response is null)
        {
            return null;
        }

        memoryCache.Set(cacheKey, response, CacheDuration);
        return response;
    }

    private static string BuildCarrierRateCacheKey(Carrier carrier, RateQueryRequest request)
    {
        var ratesEndpoint = carrier.Endpoints
            .FirstOrDefault(x => x.Operation.Equals("Rates", StringComparison.OrdinalIgnoreCase))?
            .Endpoint
            ?? "none";

        var updatedAt = carrier.UpdatedAtUtc?.Ticks ?? 0;

        return string.Create(
            System.Globalization.CultureInfo.InvariantCulture,
            $"rates:{carrier.Slug}:{carrier.Id:N}:{updatedAt}:{ratesEndpoint}:{request.Weight:F3}:{request.Length:F3}:{request.Width:F3}:{request.Height:F3}");
    }

    private async Task<ShippingRateResponseDto> QueryCarrierInternalAsync(
        Carrier carrier,
        RateQueryRequest request,
        CancellationToken cancellationToken)
    {
        var response = await TryQueryCarrierWithCacheAsync(carrier, request, cancellationToken);
        if (response is null)
        {
            throw new CarrierConflictException("Carrier does not have a supported enabled rates endpoint.");
        }

        return response;
    }
}
