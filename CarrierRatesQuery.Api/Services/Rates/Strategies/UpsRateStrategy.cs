using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.Rates.Clients;

namespace CarrierRatesQuery.Api.Services.Rates.Strategies;

public sealed class UpsRateStrategy(
    IMockUpsRatesClient mockUpsRatesClient,
    ICarrierRateAdapter<MockUpsRateResponse> upsRateAdapter) : ICarrierRateStrategy
{
    public string CarrierSlug => "ups";

    public async Task<ShippingRateResponseDto?> TryGetRatesAsync(
        Carrier carrier,
        RateQueryRequest request,
        CancellationToken cancellationToken)
    {
        var endpoint = carrier.Endpoints
            .FirstOrDefault(x => x.Operation.Equals("Rates", StringComparison.OrdinalIgnoreCase));

        if (endpoint is null)
        {
            return null;
        }

        var upsRequest = new MockUpsRateRequest(
            new MockUpsShipment(
                WeightLbs: request.Weight,
                DimensionsInches: new MockUpsDimensions(
                    Length: request.Length,
                    Width: request.Width,
                    Height: request.Height)));

        var upsResponse = await mockUpsRatesClient.GetRatesAsync(endpoint.Endpoint, upsRequest, cancellationToken);
        return upsRateAdapter.Adapt(upsResponse);
    }
}
