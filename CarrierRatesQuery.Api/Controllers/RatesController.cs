using CarrierRatesQuery.Api.Services.Rates;
using Microsoft.AspNetCore.Mvc;

namespace CarrierRatesQuery.Api.Controllers;

[ApiController]
[Route("api/rates")]
public class RatesController(IRateQueryService rateQueryService) : ControllerBase
{
    [HttpPost("query-all")]
    [ProducesResponseType(typeof(IReadOnlyList<ShippingRateResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> QueryAll([FromBody] RateQueryRequest request, CancellationToken cancellationToken)
    {
        var rates = await rateQueryService.QueryAllEnabledCarriersAsync(request, cancellationToken);
        return Ok(rates);
    }

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
