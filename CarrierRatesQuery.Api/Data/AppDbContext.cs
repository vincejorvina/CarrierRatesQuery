using CarrierRatesQuery.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarrierRatesQuery.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Carrier> Carriers => Set<Carrier>();
    public DbSet<CarrierEndpoint> CarrierEndpoints => Set<CarrierEndpoint>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<DisableRequest> DisableRequests => Set<DisableRequest>();
}
