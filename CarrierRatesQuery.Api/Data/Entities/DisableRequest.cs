namespace CarrierRatesQuery.Api.Data.Entities;

public class DisableRequest
{
    public Guid Id { get; set; }
    public Guid CarrierId { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DisableRequestStatus Status { get; set; }
    public DateTime RequestedAtUtc { get; set; }
    public string? ProcessedBy { get; set; }
    public DateTime? ProcessedAtUtc { get; set; }

    public Carrier Carrier { get; set; } = default!;
}

public enum DisableRequestStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}
