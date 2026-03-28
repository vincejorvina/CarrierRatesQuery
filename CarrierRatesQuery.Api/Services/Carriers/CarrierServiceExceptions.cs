namespace CarrierRatesQuery.Api.Services.Carriers;

public sealed class CarrierNotFoundException(Guid carrierId) : Exception($"Carrier '{carrierId}' was not found.")
{
    public Guid CarrierId { get; } = carrierId;
}

public sealed class CarrierSlugNotFoundException(string carrierSlug) : Exception($"Carrier '{carrierSlug}' was not found.")
{
    public string CarrierSlug { get; } = carrierSlug;
}

public sealed class CarrierConflictException(string message) : Exception(message)
{
}
