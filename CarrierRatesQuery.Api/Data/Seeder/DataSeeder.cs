using CarrierRatesQuery.Api.Data;
using CarrierRatesQuery.Api.Data.Entities;

namespace CarrierRatesQuery.Api.Data.Seeder;

public class DataSeeder(AppDbContext context)
{
    public void Seed()
    {
        if (context.Carriers.Any())
        {
            return;
        }

        var fedExCarrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "FedEx",
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow,
            Endpoints = [ new CarrierEndpoint
            {
                Id = Guid.NewGuid(),
                Operation = "Rates",
                Endpoint = "http://localhost:5133/api/fedex/rates"
            }]
        };

        var dhlCarrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "DHL",
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow,
            Endpoints = [ new CarrierEndpoint
            {
                Id = Guid.NewGuid(),
                Operation = "Rates",
                Endpoint = "http://localhost:5225/api/dhl/rates"
            }]
        };

        var upsCarrier = new Carrier
        {
            Id = Guid.NewGuid(),
            Name = "UPS",
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow,
            Endpoints = [ new CarrierEndpoint
            {
                Id = Guid.NewGuid(),
                Operation = "Rates",
                Endpoint = "http://localhost:5172/api/ups/shipping-rates"
            }]
        };

        context.Carriers.AddRange(fedExCarrier, dhlCarrier, upsCarrier);

        context.Shipments.Add(new Shipment
        {
            Id = Guid.NewGuid(),
            CarrierId = dhlCarrier.Id,
            Status = ShipmentStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        });

        context.DisableRequests.Add(new DisableRequest
        {
            Id = Guid.NewGuid(),
            CarrierId = upsCarrier.Id,
            RequestedBy = "user.demo",
            Reason = "maintenance",
            Status = DisableRequestStatus.Pending,
            RequestedAtUtc = DateTime.UtcNow
        });

        context.CarrierFinancialSettlements.Add(new CarrierFinancialSettlement
        {
            Id = Guid.NewGuid(),
            CarrierId = fedExCarrier.Id,
            Status = CarrierFinancialSettlementStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        });

        context.SaveChanges();
    }
}
