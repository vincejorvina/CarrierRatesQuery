namespace CarrierRatesQuery.Api.Data.Entities;

public class CarrierDisableAudit
{
    public Guid Id { get; set; }
    public Guid CarrierId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime DisabledAtUtc { get; set; }

    public Carrier Carrier { get; set; } = default!;
}
