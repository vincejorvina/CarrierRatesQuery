using CarrierRatesQuery.Api.Infrastructure;
using CarrierRatesQuery.Api.Services.Carriers;
using CarrierRatesQuery.Api.Services.DisableRequests;
using Microsoft.AspNetCore.Mvc;

namespace CarrierRatesQuery.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarriersController(
    ICarrierService carrierService,
    IDisableRequestService disableRequestService,
    IRequestRoleAccessor requestRoleAccessor) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CarrierResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var carriers = await carrierService.GetAllAsync(cancellationToken);
        return Ok(carriers);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CarrierResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var carrier = await carrierService.GetByIdAsync(id, cancellationToken);

        if (carrier is null)
        {
            return NotFound();
        }

        return Ok(carrier);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CarrierResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCarrierRequest request, CancellationToken cancellationToken)
    {
        var carrier = await carrierService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = carrier.Id }, carrier);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CarrierResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCarrierRequest request, CancellationToken cancellationToken)
    {
        var carrier = await carrierService.UpdateAsync(id, request, cancellationToken);
        return Ok(carrier);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await carrierService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPatch("{id:guid}/enable")]
    [ProducesResponseType(typeof(CarrierResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Enable(Guid id, CancellationToken cancellationToken)
    {
        var carrier = await carrierService.EnableAsync(id, cancellationToken);
        return Ok(carrier);
    }

    [HttpPatch("{id:guid}/disable")]
    [ProducesResponseType(typeof(CarrierResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Disable(Guid id, [FromBody] DisableCarrierRequest request, CancellationToken cancellationToken)
    {
        var role = requestRoleAccessor.GetRequiredRole();
        if (role != RequestRole.Admin)
        {
            throw new ForbiddenOperationException("Only administrators can disable carriers directly. Use disable-request flow for regular users.");
        }

        var carrier = await carrierService.DisableAsync(id, request, cancellationToken);
        return Ok(carrier);
    }

    [HttpGet("{id:guid}/disable-requests")]
    [ProducesResponseType(typeof(IReadOnlyList<DisableRequestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDisableRequests(Guid id, CancellationToken cancellationToken)
    {
        _ = requestRoleAccessor.GetRequiredRole();

        var requests = await disableRequestService.GetByCarrierAsync(id, cancellationToken);
        return Ok(requests);
    }

    [HttpPost("{id:guid}/disable-requests")]
    [ProducesResponseType(typeof(DisableRequestResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDisableRequest(Guid id, [FromBody] DisableCarrierRequest request, CancellationToken cancellationToken)
    {
        _ = requestRoleAccessor.GetRequiredRole();
        var requestedBy = requestRoleAccessor.GetRequestedBy();

        var disableRequest = await disableRequestService.CreateAsync(id, requestedBy, request.Reason, cancellationToken);
        return CreatedAtAction(nameof(GetDisableRequests), new { id }, disableRequest);
    }
}
