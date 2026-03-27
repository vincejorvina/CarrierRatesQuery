namespace CarrierRatesQuery.Api.Data.Entities;

public class Shipment
{
    public Guid Id { get; set; }
    public Guid CarrierId { get; set; }
    public ShipmentStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Carrier Carrier { get; set; } = default!;
}

public enum ShipmentStatus
{
    Pending = 0,
    Completed = 1
}
