using CarrierRatesQuery.Api.Services.Carriers;
using CarrierRatesQuery.Api.Services.CarrierEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CarrierRatesQuery.Api.Infrastructure;

public sealed class ApiExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ValidationException validationException:
                var errors = validationException.Errors
                    .GroupBy(x => x.PropertyName.ToLowerInvariant(), x => x.ErrorMessage)
                    .ToDictionary(group => group.Key, group => group.Distinct().ToArray());

                var validationProblemDetails = new ValidationProblemDetails(errors)
                {
                    Title = "Validation failed",
                    Status = StatusCodes.Status400BadRequest
                };

                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(validationProblemDetails, cancellationToken);
                return true;

            case CarrierNotFoundException:
            case CarrierSlugNotFoundException:
            case CarrierEndpointNotFoundException:
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                return true;

            case CarrierConflictException conflictException:
                var conflictProblemDetails = new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = conflictException.Message,
                    Status = StatusCodes.Status409Conflict
                };

                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                await httpContext.Response.WriteAsJsonAsync(conflictProblemDetails, cancellationToken);
                return true;

            case HttpRequestException httpRequestException:
                var upstreamProblemDetails = new ProblemDetails
                {
                    Title = "Upstream service error",
                    Detail = httpRequestException.Message,
                    Status = StatusCodes.Status502BadGateway
                };

                httpContext.Response.StatusCode = StatusCodes.Status502BadGateway;
                await httpContext.Response.WriteAsJsonAsync(upstreamProblemDetails, cancellationToken);
                return true;

            default:
                return false;
        }
    }
}
