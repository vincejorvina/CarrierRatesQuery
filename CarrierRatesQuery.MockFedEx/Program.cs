using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/fedex/rates", ([FromBody] FedExRateRequest request) =>
{
    var weightMultiplier = Math.Max(request.Package.Weight, 1m) * 0.15m;

    var response = new FedExRateResponse(
        Carrier: "FedEx",
        ServiceOptions:
        [
            new FedExServiceOption(
                ServiceName: "FedEx Ground",
                EstimatedDelivery: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)).ToString("yyyy-MM-dd"),
                Rate: Math.Round(12.34m + weightMultiplier, 2)
            ),
            new FedExServiceOption(
                ServiceName: "FedEx 2Day",
                EstimatedDelivery: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)).ToString("yyyy-MM-dd"),
                Rate: Math.Round(25.67m + weightMultiplier, 2)
            ),
            new FedExServiceOption(
                ServiceName: "FedEx Overnight",
                EstimatedDelivery: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)).ToString("yyyy-MM-dd"),
                Rate: Math.Round(45.89m + weightMultiplier, 2)
            )
        ]
    );

    return Results.Ok(response);
})
.WithName("PostFedExRates")
.Accepts<FedExRateRequest>("application/json")
.Produces<FedExRateResponse>(StatusCodes.Status200OK)
.WithOpenApi();

await app.RunAsync();

public sealed record FedExRateRequest(FedExPackage Package);

public sealed record FedExPackage(decimal Weight, FedExDimensions Dimensions);

public sealed record FedExDimensions(decimal Length, decimal Width, decimal Height);

public sealed record FedExRateResponse(string Carrier, IReadOnlyList<FedExServiceOption> ServiceOptions);

public sealed record FedExServiceOption(string ServiceName, string EstimatedDelivery, decimal Rate);
