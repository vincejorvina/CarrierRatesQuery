using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Seeder;

namespace CarrierRatesQuery.Api.Infrastructure;

public static class ApplicationInitializationExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
        seeder.Seed();
    }
}
