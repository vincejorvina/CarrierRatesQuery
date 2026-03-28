using CarrierRatesQuery.Api.Services.CarrierEndpoints;
using Microsoft.AspNetCore.Mvc;

namespace CarrierRatesQuery.Api.Controllers;

[ApiController]
[Route("api/carriers/{carrierId:guid}/endpoints")]
public class CarrierEndpointsController(ICarrierEndpointService carrierEndpointService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CarrierEndpointResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAll(Guid carrierId, CancellationToken cancellationToken)
    {
        var endpoints = await carrierEndpointService.GetAllAsync(carrierId, cancellationToken);
        return Ok(endpoints);
    }

    [HttpGet("{endpointId:guid}")]
    [ProducesResponseType(typeof(CarrierEndpointResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid carrierId, Guid endpointId, CancellationToken cancellationToken)
    {
        var endpoint = await carrierEndpointService.GetByIdAsync(carrierId, endpointId, cancellationToken);
        return Ok(endpoint);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CarrierEndpointResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(Guid carrierId, [FromBody] CreateCarrierEndpointRequest request, CancellationToken cancellationToken)
    {
        var endpoint = await carrierEndpointService.CreateAsync(carrierId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { carrierId, endpointId = endpoint.Id }, endpoint);
    }

    [HttpPut("{endpointId:guid}")]
    [ProducesResponseType(typeof(CarrierEndpointResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid carrierId, Guid endpointId, [FromBody] UpdateCarrierEndpointRequest request, CancellationToken cancellationToken)
    {
        var endpoint = await carrierEndpointService.UpdateAsync(carrierId, endpointId, request, cancellationToken);
        return Ok(endpoint);
    }

    [HttpDelete("{endpointId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid carrierId, Guid endpointId, CancellationToken cancellationToken)
    {
        await carrierEndpointService.DeleteAsync(carrierId, endpointId, cancellationToken);
        return NoContent();
    }
}
