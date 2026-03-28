using CarrierRatesQuery.Api.Infrastructure;
using CarrierRatesQuery.Api.Services.DisableRequests;
using Microsoft.AspNetCore.Mvc;

namespace CarrierRatesQuery.Api.Controllers;

[ApiController]
[Route("api/disable-requests")]
public class DisableRequestsController(
    IDisableRequestService disableRequestService,
    IRequestRoleAccessor requestRoleAccessor) : ControllerBase
{
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
