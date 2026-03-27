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

app.MapPost("/api/fedex/rates", (FedExRateRequest request) =>
{
    var response = new FedExRateResponse(
        Carrier: "FedEx",
        ServiceOptions:
        [
            new FedExServiceOption(
                ServiceName: "FedEx Ground",
                EstimatedDelivery: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(5)).ToString("yyyy-MM-dd"),
                Rate: 12.34m
            ),
            new FedExServiceOption(
                ServiceName: "FedEx 2Day",
                EstimatedDelivery: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)).ToString("yyyy-MM-dd"),
                Rate: 25.67m
            ),
            new FedExServiceOption(
                ServiceName: "FedEx Overnight",
                EstimatedDelivery: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)).ToString("yyyy-MM-dd"),
                Rate: 45.89m
            )
        ]
    );

    return Results.Ok(response);
})
.WithName("GetFedExRates")
.WithOpenApi();

await app.RunAsync();

internal sealed record FedExRateRequest(FedExLocation Origin, FedExLocation Destination, FedExPackage Package);

internal sealed record FedExLocation(string PostalCode, string CountryCode);

internal sealed record FedExPackage(decimal Weight, FedExDimensions Dimensions);

internal sealed record FedExDimensions(decimal Length, decimal Width, decimal Height);

internal sealed record FedExRateResponse(string Carrier, IReadOnlyList<FedExServiceOption> ServiceOptions);

internal sealed record FedExServiceOption(string ServiceName, string EstimatedDelivery, decimal Rate);
