using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Seeder;
using CarrierRatesQuery.Api.Services.CarrierEndpoints;
using CarrierRatesQuery.Api.Services.Carriers;
using CarrierRatesQuery.Api.Services.DisableRequests;
using CarrierRatesQuery.Api.Services.Rates;
using CarrierRatesQuery.Api.Services.Rates.Adapters;
using CarrierRatesQuery.Api.Services.Rates.Clients;
using CarrierRatesQuery.Api.Services.Rates.Strategies;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CarrierRatesQuery.Api.Infrastructure;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddMemoryCache();
        services.AddHttpContextAccessor();
        services.AddScoped<IRequestRoleAccessor, RequestRoleAccessor>();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase("CarrierRatesQueryDb"));

        services.AddScoped<DataSeeder>();

        services.AddScoped<ICarrierService, CarrierService>();
        services.AddScoped<ICarrierEndpointService, CarrierEndpointService>();
        services.AddScoped<IDisableRequestService, DisableRequestService>();
        services.AddScoped<IRateQueryService, RateQueryService>();

        services.AddScoped<ICarrierRateStrategyResolver, CarrierRateStrategyResolver>();
        services.AddScoped<ICarrierRateStrategy, DhlRateStrategy>();
        services.AddScoped<ICarrierRateStrategy, FedExRateStrategy>();
        services.AddScoped<ICarrierRateStrategy, UpsRateStrategy>();
        services.AddScoped<ICarrierRateStrategy, LbcRateStrategy>();

        services.AddScoped<ICarrierRateAdapter<MockDhlRateResponse>, DhlRateAdapter>();
        services.AddScoped<ICarrierRateAdapter<MockFedExRateResponse>, FedExRateAdapter>();
        services.AddScoped<ICarrierRateAdapter<MockUpsRateResponse>, UpsRateAdapter>();
        services.AddScoped<ICarrierRateAdapter<MockLbcRateResponse>, LbcRateAdapter>();

        services.AddHttpClient<IMockDhlRatesClient, MockDhlRatesClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddStandardResilienceHandler();

        services.AddHttpClient<IMockFedExRatesClient, MockFedExRatesClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddStandardResilienceHandler();

        services.AddHttpClient<IMockUpsRatesClient, MockUpsRatesClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddStandardResilienceHandler();

        services.AddHttpClient<IMockLbcRatesClient, MockLbcRatesClient>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
        })
        .AddStandardResilienceHandler();

        services.AddScoped<IValidator<CreateCarrierRequest>, CreateCarrierRequestValidator>();
        services.AddScoped<IValidator<UpdateCarrierRequest>, UpdateCarrierRequestValidator>();
        services.AddScoped<IValidator<DisableCarrierRequest>, DisableCarrierRequestValidator>();
        services.AddScoped<IValidator<CreateCarrierEndpointRequest>, CreateCarrierEndpointRequestValidator>();
        services.AddScoped<IValidator<UpdateCarrierEndpointRequest>, UpdateCarrierEndpointRequestValidator>();
        services.AddScoped<IValidator<RateQueryRequest>, RateQueryRequestValidator>();

        return services;
    }
}
