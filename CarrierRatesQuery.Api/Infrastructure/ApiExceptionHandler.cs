using CarrierRatesQuery.Api.Services.Carriers;
using CarrierRatesQuery.Api.Services.CarrierEndpoints;
using CarrierRatesQuery.Api.Services.DisableRequests;
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
            case DisableRequestNotFoundException:
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

            case MissingRequestHeaderException missingHeaderException:
                var missingHeaderProblemDetails = new ProblemDetails
                {
                    Title = "Missing header",
                    Detail = missingHeaderException.Message,
                    Status = StatusCodes.Status400BadRequest
                };

                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(missingHeaderProblemDetails, cancellationToken);
                return true;

            case InvalidRequestHeaderException invalidHeaderException:
                var invalidHeaderProblemDetails = new ProblemDetails
                {
                    Title = "Invalid header",
                    Detail = invalidHeaderException.Message,
                    Status = StatusCodes.Status400BadRequest
                };

                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(invalidHeaderProblemDetails, cancellationToken);
                return true;

            case ForbiddenOperationException forbiddenOperationException:
                var forbiddenProblemDetails = new ProblemDetails
                {
                    Title = "Forbidden",
                    Detail = forbiddenOperationException.Message,
                    Status = StatusCodes.Status403Forbidden
                };

                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsJsonAsync(forbiddenProblemDetails, cancellationToken);
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
