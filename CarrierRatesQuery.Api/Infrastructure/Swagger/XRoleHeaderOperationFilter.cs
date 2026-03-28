using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CarrierRatesQuery.Api.Infrastructure.Swagger;

public sealed class XRoleHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var relativePath = context.ApiDescription.RelativePath ?? string.Empty;
        if (!relativePath.Contains("disable", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        operation.Parameters ??= new List<OpenApiParameter>();

        if (!operation.Parameters.Any(x =>
            x.In == ParameterLocation.Header &&
            string.Equals(x.Name, "X-Role", StringComparison.OrdinalIgnoreCase)))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Role",
                In = ParameterLocation.Header,
                Description = "Required for disable operations. Use 'Admin' for direct disable/approve/reject. Use 'User' to submit a disable request.",
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new Microsoft.OpenApi.Any.OpenApiString("Admin")
                }
            });
        }

        if (!operation.Parameters.Any(x =>
            x.In == ParameterLocation.Header &&
            string.Equals(x.Name, "X-Requested-By", StringComparison.OrdinalIgnoreCase)))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Requested-By",
                In = ParameterLocation.Header,
                Description = "Optional. Identifies the user making the request. Defaults to the role name if not provided.",
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = "string"
                }
            });
        }
    }
}
