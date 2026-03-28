using CarrierRatesQuery.Api.Services.Rates;
using Microsoft.AspNetCore.Mvc;

namespace CarrierRatesQuery.Api.Controllers;

/// <summary>
/// Queries shipping rates from enabled carrier APIs and returns them in a unified format.
/// </summary>
[ApiController]
[Route("api/rates")]
public class RatesController(IRateQueryService rateQueryService) : ControllerBase
{
    /// <summary>
    /// Queries shipping rates from all enabled carriers and returns aggregated results.
    /// </summary>
    /// <param name="request">Package dimensions and weight.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Shipping rates from each enabled carrier.</returns>
    [HttpPost("query-all")]
    [ProducesResponseType(typeof(IReadOnlyList<ShippingRateResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> QueryAll([FromBody] RateQueryRequest request, CancellationToken cancellationToken)
    {
        var rates = await rateQueryService.QueryAllEnabledCarriersAsync(request, cancellationToken);
        return Ok(rates);
    }

    /// <summary>
    /// Queries shipping rates for a specific carrier by its ID.
    /// </summary>
    /// <param name="carrierId">The carrier's unique identifier.</param>
    /// <param name="request">Package dimensions and weight.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Shipping rates from the specified carrier.</returns>
    [HttpPost("query/{carrierId:guid}")]
    [ProducesResponseType(typeof(ShippingRateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> QuerySingle(Guid carrierId, [FromBody] RateQueryRequest request, CancellationToken cancellationToken)
    {
        var rate = await rateQueryService.QueryCarrierAsync(carrierId, request, cancellationToken);
        return Ok(rate);
    }

    /// <summary>
    /// Queries shipping rates for a specific carrier by its slug (e.g. "fedex", "ups", "dhl").
    /// </summary>
    /// <param name="carrierSlug">The carrier's URL-friendly name identifier.</param>
    /// <param name="request">Package dimensions and weight.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Shipping rates from the specified carrier.</returns>
    [HttpPost("query/slug/{carrierSlug}")]
    [ProducesResponseType(typeof(ShippingRateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> QueryBySlug(string carrierSlug, [FromBody] RateQueryRequest request, CancellationToken cancellationToken)
    {
        var rate = await rateQueryService.QueryCarrierBySlugAsync(carrierSlug, request, cancellationToken);
        return Ok(rate);
    }
}
