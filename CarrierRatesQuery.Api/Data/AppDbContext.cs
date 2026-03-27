using CarrierRatesQuery.Api.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarrierRatesQuery.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Carrier> Carriers => Set<Carrier>();
    public DbSet<CarrierEndpoint> CarrierEndpoints => Set<CarrierEndpoint>();
    public DbSet<Shipment> Shipments => Set<Shipment>();
    public DbSet<DisableRequest> DisableRequests => Set<DisableRequest>();
    public DbSet<CarrierDisableAudit> CarrierDisableAudits => Set<CarrierDisableAudit>();
    public DbSet<CarrierFinancialSettlement> CarrierFinancialSettlements => Set<CarrierFinancialSettlement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Carrier>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(100);

            entity.HasMany(x => x.Endpoints)
                .WithOne(x => x.Carrier)
                .HasForeignKey(x => x.CarrierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Shipments)
                .WithOne(x => x.Carrier)
                .HasForeignKey(x => x.CarrierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.DisableRequests)
                .WithOne(x => x.Carrier)
                .HasForeignKey(x => x.CarrierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.DisableAudits)
                .WithOne(x => x.Carrier)
                .HasForeignKey(x => x.CarrierId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.FinancialSettlements)
                .WithOne(x => x.Carrier)
                .HasForeignKey(x => x.CarrierId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CarrierEndpoint>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Operation).IsRequired().HasMaxLength(50);
            entity.Property(x => x.Endpoint).IsRequired().HasMaxLength(500);
        });

        modelBuilder.Entity<Shipment>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<int>();
        });

        modelBuilder.Entity<DisableRequest>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<int>();
            entity.Property(x => x.RequestedBy).IsRequired().HasMaxLength(100);
            entity.Property(x => x.Reason).IsRequired().HasMaxLength(200);
            entity.Property(x => x.ProcessedBy).HasMaxLength(100);
        });

        modelBuilder.Entity<CarrierDisableAudit>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Reason).IsRequired().HasMaxLength(200);
        });

        modelBuilder.Entity<CarrierFinancialSettlement>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion<int>();
        });
    }
}
