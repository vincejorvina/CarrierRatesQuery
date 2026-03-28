using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Seeder;
using CarrierRatesQuery.Api.Infrastructure;
using CarrierRatesQuery.Api.Services.CarrierEndpoints;
using CarrierRatesQuery.Api.Services.Carriers;
using CarrierRatesQuery.Api.Services.Rates.Adapters;
using CarrierRatesQuery.Api.Services.Rates.Clients;
using CarrierRatesQuery.Api.Services.Rates.Strategies;
using CarrierRatesQuery.Api.Services.Rates;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseInMemoryDatabase("CarrierRatesQueryDb"));
builder.Services.AddScoped<DataSeeder>();
builder.Services.AddScoped<ICarrierService, CarrierService>();
builder.Services.AddScoped<ICarrierEndpointService, CarrierEndpointService>();
builder.Services.AddScoped<IRateQueryService, RateQueryService>();
builder.Services.AddScoped<ICarrierRateStrategyResolver, CarrierRateStrategyResolver>();
builder.Services.AddScoped<ICarrierRateStrategy, DhlRateStrategy>();
builder.Services.AddScoped<ICarrierRateStrategy, FedExRateStrategy>();
builder.Services.AddScoped<ICarrierRateStrategy, UpsRateStrategy>();
builder.Services.AddScoped<ICarrierRateAdapter<MockDhlRateResponse>, DhlRateAdapter>();
builder.Services.AddScoped<ICarrierRateAdapter<MockFedExRateResponse>, FedExRateAdapter>();
builder.Services.AddScoped<ICarrierRateAdapter<MockUpsRateResponse>, UpsRateAdapter>();
builder.Services.AddHttpClient<IMockDhlRatesClient, MockDhlRatesClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHttpClient<IMockFedExRatesClient, MockFedExRatesClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHttpClient<IMockUpsRatesClient, MockUpsRatesClient>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<IValidator<CreateCarrierRequest>, CreateCarrierRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateCarrierRequest>, UpdateCarrierRequestValidator>();
builder.Services.AddScoped<IValidator<DisableCarrierRequest>, DisableCarrierRequestValidator>();
builder.Services.AddScoped<IValidator<CreateCarrierEndpointRequest>, CreateCarrierEndpointRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateCarrierEndpointRequest>, UpdateCarrierEndpointRequestValidator>();
builder.Services.AddScoped<IValidator<RateQueryRequest>, RateQueryRequestValidator>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    seeder.Seed();
}

await app.RunAsync();
