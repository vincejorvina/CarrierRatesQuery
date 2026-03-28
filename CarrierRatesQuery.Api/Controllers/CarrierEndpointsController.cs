using CarrierRatesQuery.Api.Services.CarrierEndpoints;
using Microsoft.AspNetCore.Mvc;

namespace CarrierRatesQuery.Api.Controllers;

/// <summary>
/// Manages API endpoint configurations for a specific carrier (e.g. rate lookup URLs).
/// </summary>
[ApiController]
[Route("api/carriers/{carrierId:guid}/endpoints")]
public class CarrierEndpointsController(ICarrierEndpointService carrierEndpointService) : ControllerBase
{
    /// <summary>
    /// Returns all endpoints for a carrier, ordered by operation name.
    /// </summary>
    /// <param name="carrierId">The carrier's unique identifier.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CarrierEndpointResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(Guid carrierId, CancellationToken cancellationToken)
    {
        var endpoints = await carrierEndpointService.GetAllAsync(carrierId, cancellationToken);
        return Ok(endpoints);
    }

    /// <summary>
    /// Returns a specific endpoint for a carrier.
    /// </summary>
    /// <param name="carrierId">The carrier's unique identifier.</param>
    /// <param name="endpointId">The endpoint's unique identifier.</param>
    [HttpGet("{endpointId:guid}")]
    [ProducesResponseType(typeof(CarrierEndpointResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid carrierId, Guid endpointId, CancellationToken cancellationToken)
    {
        var endpoint = await carrierEndpointService.GetByIdAsync(carrierId, endpointId, cancellationToken);
        return Ok(endpoint);
    }

    /// <summary>
    /// Adds a new API endpoint to a carrier.
    /// </summary>
    /// <param name="carrierId">The carrier's unique identifier.</param>
    /// <param name="request">The operation name (e.g. "Rates") and the endpoint URL.</param>
    [HttpPost]
    [ProducesResponseType(typeof(CarrierEndpointResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(Guid carrierId, [FromBody] CreateCarrierEndpointRequest request, CancellationToken cancellationToken)
    {
        var endpoint = await carrierEndpointService.CreateAsync(carrierId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { carrierId, endpointId = endpoint.Id }, endpoint);
    }

    /// <summary>
    /// Updates an existing endpoint for a carrier.
    /// </summary>
    /// <param name="carrierId">The carrier's unique identifier.</param>
    /// <param name="endpointId">The endpoint's unique identifier.</param>
    /// <param name="request">Updated operation name and endpoint URL.</param>
    [HttpPut("{endpointId:guid}")]
    [ProducesResponseType(typeof(CarrierEndpointResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid carrierId, Guid endpointId, [FromBody] UpdateCarrierEndpointRequest request, CancellationToken cancellationToken)
    {
        var endpoint = await carrierEndpointService.UpdateAsync(carrierId, endpointId, request, cancellationToken);
        return Ok(endpoint);
    }

    /// <summary>
    /// Removes an endpoint from a carrier.
    /// </summary>
    /// <param name="carrierId">The carrier's unique identifier.</param>
    /// <param name="endpointId">The endpoint's unique identifier.</param>
    [HttpDelete("{endpointId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid carrierId, Guid endpointId, CancellationToken cancellationToken)
    {
        await carrierEndpointService.DeleteAsync(carrierId, endpointId, cancellationToken);
        return NoContent();
    }
}
