namespace CarrierRatesQuery.Api.Data.Entities;

public class CarrierFinancialSettlement
{
    public Guid Id { get; set; }
    public Guid CarrierId { get; set; }
    public CarrierFinancialSettlementStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Carrier Carrier { get; set; } = default!;
}

public enum CarrierFinancialSettlementStatus
{
    Pending = 0,
    Settled = 1
}
