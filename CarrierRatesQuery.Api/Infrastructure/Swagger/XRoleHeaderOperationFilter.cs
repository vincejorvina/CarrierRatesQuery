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

        var hasRoleHeader = operation.Parameters.Any(x =>
            x.In == ParameterLocation.Header &&
            string.Equals(x.Name, "X-Role", StringComparison.OrdinalIgnoreCase));

        if (hasRoleHeader)
        {
            return;
        }

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Role",
            In = ParameterLocation.Header,
            Description = "Role header for admin-restricted actions. Use 'Admin' to disable carriers.",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Default = new Microsoft.OpenApi.Any.OpenApiString("Admin")
            }
        });
    }
}
