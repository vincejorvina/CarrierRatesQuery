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

app.MapPost("/api/lbc/rates", ([FromBody] LbcRateRequest request) =>
{
    var package = request.Package;
    var volume = Math.Max(package.Length * package.Width * package.Height, 1m);
    var surcharge = Math.Max(package.Weight, 1m) * 0.12m + volume * 0.002m;

    var response = new LbcRateResponse(
        Carrier: "LBC Express",
        ServiceOptions:
        [
            new LbcServiceOption("LBC Regular", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(4)).ToString("yyyy-MM-dd"), Math.Round(9.99m + surcharge, 2)),
            new LbcServiceOption("LBC Priority", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)).ToString("yyyy-MM-dd"), Math.Round(18.49m + surcharge, 2))
        ]);

    return Results.Ok(response);
})
.WithName("PostLbcRates")
.Accepts<LbcRateRequest>("application/json")
.Produces<LbcRateResponse>(StatusCodes.Status200OK)
.WithOpenApi();

await app.RunAsync();

public sealed record LbcRateRequest(LbcPackage Package);

public sealed record LbcPackage(decimal Weight, decimal Length, decimal Width, decimal Height);

public sealed record LbcRateResponse(string Carrier, IReadOnlyList<LbcServiceOption> ServiceOptions);

public sealed record LbcServiceOption(string ServiceName, string EstimatedDelivery, decimal Rate);
