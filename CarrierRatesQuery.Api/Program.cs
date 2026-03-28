using System.Reflection;
using CarrierRatesQuery.Api.Infrastructure;
using CarrierRatesQuery.Api.Infrastructure.Swagger;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Carrier Rates Query API",
        Version = "v1",
        Description = """
            Aggregates shipping rates from multiple carrier APIs (FedEx, UPS, DHL) 
            and returns them in a unified format. Supports carrier management, 
            endpoint configuration, and a disable-request approval workflow.
            """
    });

    options.TagActionsBy(api =>
    {
        var path = api.RelativePath?.ToLowerInvariant() ?? string.Empty;

        if (path.StartsWith("api/rates"))
            return ["Rates"];
        if (path.StartsWith("api/carriers/") && path.Contains("/endpoints"))
            return ["Carrier Endpoints"];
        if (path.StartsWith("api/carriers"))
            return ["Carriers"];
        if (path.StartsWith("api/disable-requests"))
            return ["Disable Requests"];

        return [api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] ?? "Default"];
    });

    options.DocInclusionPredicate((name, api) => true);

    options.AddSecurityDefinition("X-Role", new OpenApiSecurityScheme
    {
        Name = "X-Role",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Role identifier. Use 'Admin' for admin-restricted operations.",
        Scheme = "X-Role"
    });

    options.OperationFilter<XRoleHeaderOperationFilter>();

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});
builder.Services.AddApiServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllers();

await app.InitializeDatabaseAsync();

await app.RunAsync();
