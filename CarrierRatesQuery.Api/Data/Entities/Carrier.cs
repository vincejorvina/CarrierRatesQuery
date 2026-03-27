namespace CarrierRatesQuery.Api.Data.Entities;

public class Carrier
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug => ConvertToSlug(Name);
    public bool IsEnabled { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public ICollection<CarrierEndpoint> Endpoints { get; set; } = [];
    public ICollection<Shipment> Shipments { get; set; } = [];
    public ICollection<DisableRequest> DisableRequests { get; set; } = [];
    public ICollection<CarrierDisableAudit> DisableAudits { get; set; } = [];
    public ICollection<CarrierFinancialSettlement> FinancialSettlements { get; set; } = [];

    private static string ConvertToSlug(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        return new string(
        [.. name
            .ToLowerInvariant()
            .Where(c => (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
        ]);
    }
}
