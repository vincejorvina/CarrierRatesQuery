using CarrierRatesQuery.Api.Infrastructure;
using CarrierRatesQuery.Api.Services.DisableRequests;
using Microsoft.AspNetCore.Mvc;

namespace CarrierRatesQuery.Api.Controllers;

/// <summary>
/// Manages the approval workflow for carrier disable requests. Admin only.
/// </summary>
[ApiController]
[Route("api/disable-requests")]
public class DisableRequestsController(
    IDisableRequestService disableRequestService,
    IRequestRoleAccessor requestRoleAccessor) : ControllerBase
{
    /// <summary>
    /// Returns all disable requests ordered by requested date (newest first).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<DisableRequestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        _ = requestRoleAccessor.GetRequiredRole();
        var requests = await disableRequestService.GetAllAsync(cancellationToken);
        return Ok(requests);
    }

    /// <summary>
    /// Approves a pending disable request, which disables the associated carrier. Enforces all disable business rules.
    /// </summary>
    /// <param name="disableRequestId">The disable request's unique identifier.</param>
    [HttpPatch("{disableRequestId:guid}/approve")]
    [ProducesResponseType(typeof(DisableRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(Guid disableRequestId, CancellationToken cancellationToken)
    {
        var role = requestRoleAccessor.GetRequiredRole();
        if (role != RequestRole.Admin)
        {
            throw new ForbiddenOperationException("Only administrators can approve disable requests.");
        }

        var processedBy = requestRoleAccessor.GetRequestedBy();
        var result = await disableRequestService.ApproveAsync(disableRequestId, processedBy, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Rejects a pending disable request. The carrier remains enabled.
    /// </summary>
    /// <param name="disableRequestId">The disable request's unique identifier.</param>
    [HttpPatch("{disableRequestId:guid}/reject")]
    [ProducesResponseType(typeof(DisableRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Reject(Guid disableRequestId, CancellationToken cancellationToken)
    {
        var role = requestRoleAccessor.GetRequiredRole();
        if (role != RequestRole.Admin)
        {
            throw new ForbiddenOperationException("Only administrators can reject disable requests.");
        }

        var processedBy = requestRoleAccessor.GetRequestedBy();
        var result = await disableRequestService.RejectAsync(disableRequestId, processedBy, cancellationToken);
        return Ok(result);
    }
}
