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

app.MapPost("/api/ups/shipping-rates", ([FromBody] UpsRateRequest request) =>
{
    var shipment = request.Shipment;
    var volume = Math.Max(shipment.DimensionsInches.Length * shipment.DimensionsInches.Width * shipment.DimensionsInches.Height, 1m);
    var surcharge = Math.Max(shipment.WeightLbs, 1m) * 0.2m + volume * 0.003m;

    var response = new UpsRateResponse(
        Company: "UPS",
        Services:
        [
            new UpsServiceOption("UPS Ground", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)).ToString("yyyy-MM-dd"), Math.Round(15.20m + surcharge, 2)),
            new UpsServiceOption("UPS 2nd Day Air", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)).ToString("yyyy-MM-dd"), Math.Round(28.40m + surcharge, 2)),
            new UpsServiceOption("UPS Next Day Air", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)).ToString("yyyy-MM-dd"), Math.Round(52.75m + surcharge, 2))
        ]);

    return Results.Ok(response);
})
.WithName("PostUpsShippingRates")
.Accepts<UpsRateRequest>("application/json")
.Produces<UpsRateResponse>(StatusCodes.Status200OK)
.WithOpenApi();

await app.RunAsync();

public sealed record UpsRateRequest(UpsShipment Shipment);

public sealed record UpsShipment(decimal WeightLbs, UpsDimensions DimensionsInches);

public sealed record UpsDimensions(decimal Length, decimal Width, decimal Height);

public sealed record UpsRateResponse(string Company, IReadOnlyList<UpsServiceOption> Services);

public sealed record UpsServiceOption(string Service, string Eta, decimal Cost);
