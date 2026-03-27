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

app.MapPost("/api/dhl/rates", ([FromBody] DhlRateRequest request) =>
{
    var parcel = request.Parcel;
    var volume = Math.Max(parcel.SizeCm.Length * parcel.SizeCm.Width * parcel.SizeCm.Height, 1m);
    var surcharge = Math.Max(parcel.WeightKg, 1m) * 0.18m + volume * 0.0015m;

    var response = new DhlRateResponse(
        Provider: "DHL",
        Options:
        [
            new DhlServiceOption("DHL Economy Select", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(6)).ToString("yyyy-MM-dd"), Math.Round(11.00m + surcharge, 2)),
            new DhlServiceOption("DHL Express Worldwide", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)).ToString("yyyy-MM-dd"), Math.Round(22.50m + surcharge, 2)),
            new DhlServiceOption("DHL Same Day", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)).ToString("yyyy-MM-dd"), Math.Round(35.00m + surcharge, 2))
        ]);

    return Results.Ok(response);
})
.WithName("PostDhlRates")
.Accepts<DhlRateRequest>("application/json")
.Produces<DhlRateResponse>(StatusCodes.Status200OK)
.WithOpenApi();

await app.RunAsync();

public sealed record DhlRateRequest(DhlParcel Parcel);

public sealed record DhlParcel(decimal WeightKg, DhlDimensions SizeCm);

public sealed record DhlDimensions(decimal Length, decimal Width, decimal Height);

public sealed record DhlRateResponse(string Provider, IReadOnlyList<DhlServiceOption> Options);

public sealed record DhlServiceOption(string Name, string DeliveryDate, decimal Price);
