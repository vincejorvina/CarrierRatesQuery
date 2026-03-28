using CarrierRatesQuery.Api.Infrastructure;
using CarrierRatesQuery.Api.Services.Carriers;
using CarrierRatesQuery.Api.Services.DisableRequests;
using Microsoft.AspNetCore.Mvc;

namespace CarrierRatesQuery.Api.Controllers;

/// <summary>
/// Manages carrier configurations including creation, updates, enable/disable, and disable-request workflows.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CarriersController(
    ICarrierService carrierService,
    IDisableRequestService disableRequestService,
    IRequestRoleAccessor requestRoleAccessor) : ControllerBase
{
    /// <summary>
    /// Returns all carriers ordered by name.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CarrierResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var carriers = await carrierService.GetAllAsync(cancellationToken);
        return Ok(carriers);
    }

    /// <summary>
    /// Returns a carrier by its ID, including configured endpoints.
    /// </summary>
    /// <param name="id">The carrier's unique identifier.</param>
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

    /// <summary>
    /// Creates a new carrier.
    /// </summary>
    /// <param name="request">Carrier name and enabled state.</param>
    [HttpPost]
    [ProducesResponseType(typeof(CarrierResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateCarrierRequest request, CancellationToken cancellationToken)
    {
        var carrier = await carrierService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = carrier.Id }, carrier);
    }

    /// <summary>
    /// Updates a carrier's name and enabled state.
    /// </summary>
    /// <param name="id">The carrier's unique identifier.</param>
    /// <param name="request">Updated carrier name and enabled state.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CarrierResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCarrierRequest request, CancellationToken cancellationToken)
    {
        var carrier = await carrierService.UpdateAsync(id, request, cancellationToken);
        return Ok(carrier);
    }

    /// <summary>
    /// Deletes a carrier. The carrier must be disabled before it can be deleted.
    /// </summary>
    /// <param name="id">The carrier's unique identifier.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await carrierService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Enables a carrier so it participates in rate queries.
    /// </summary>
    /// <param name="id">The carrier's unique identifier.</param>
    [HttpPatch("{id:guid}/enable")]
    [ProducesResponseType(typeof(CarrierResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Enable(Guid id, CancellationToken cancellationToken)
    {
        var carrier = await carrierService.EnableAsync(id, cancellationToken);
        return Ok(carrier);
    }

    /// <summary>
    /// Disables a carrier. Admin only. Enforces business rules: cannot disable the only active carrier,
    /// a carrier with pending shipments, or a carrier with pending financial settlements. A reason must be provided and is logged as an audit record.
    /// </summary>
    /// <param name="id">The carrier's unique identifier.</param>
    /// <param name="request">The reason for disabling.</param>
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

    /// <summary>
    /// Returns all disable requests for a carrier, ordered newest first.
    /// </summary>
    /// <param name="id">The carrier's unique identifier.</param>
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

    /// <summary>
    /// Submits a request to disable a carrier. Any authenticated user can submit; an admin must approve or reject.
    /// </summary>
    /// <param name="id">The carrier's unique identifier.</param>
    /// <param name="request">The reason for the disable request.</param>
    [HttpPost("{id:guid}/disable-requests")]
    [ProducesResponseType(typeof(DisableRequestResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateDisableRequest(Guid id, [FromBody] DisableCarrierRequest request, CancellationToken cancellationToken)
    {
        _ = requestRoleAccessor.GetRequiredRole();
        var requestedBy = requestRoleAccessor.GetRequestedBy();

        var disableRequest = await disableRequestService.CreateAsync(id, requestedBy, request.Reason, cancellationToken);
        return CreatedAtAction(nameof(GetDisableRequests), new { id }, disableRequest);
    }
}
