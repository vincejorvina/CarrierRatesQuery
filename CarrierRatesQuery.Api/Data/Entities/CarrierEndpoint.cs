namespace CarrierRatesQuery.Api.Data.Entities;

public class CarrierEndpoint
{
    public Guid Id { get; set; }
    public Guid CarrierId { get; set; }

    public string Operation {  get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;

    public Carrier Carrier { get; set; } = default!;
}
