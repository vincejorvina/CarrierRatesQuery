using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Entities;

namespace CarrierRatesQuery.Api.Data.Seeder;

public class DataSeeder(AppDbContext context)
{
    public void Seed()
    {
        context.Carriers.Add(new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "FedEx",
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow,
            Endpoints = [ new CarrierEndpoint
            {
                Id = Guid.NewGuid(),
                Operation = "Rates",
                Endpoint = "https://api.fedex.com/rates"
            }]
        });

        context.Carriers.Add(new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "DHL",
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow,
            Endpoints = [ new CarrierEndpoint
            {
                Id = Guid.NewGuid(),
                Operation = "Rates",
                Endpoint = "https://api.dhl.com/rates"
            }]
        });

        context.Carriers.Add(new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "UPS",
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow,
            Endpoints = [ new CarrierEndpoint
            {
                Id = Guid.NewGuid(),
                Operation = "Rates",
                Endpoint = "https://api.ups.com/rates"
            }]
        });

        context.SaveChanges();
    }
}
