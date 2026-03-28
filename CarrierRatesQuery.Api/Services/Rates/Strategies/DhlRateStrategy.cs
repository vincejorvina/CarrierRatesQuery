using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.Rates.Clients;

namespace CarrierRatesQuery.Api.Services.Rates.Strategies;

public sealed class DhlRateStrategy(
    IMockDhlRatesClient mockDhlRatesClient,
    ICarrierRateAdapter<MockDhlRateResponse> dhlRateAdapter) : ICarrierRateStrategy
{
    public string CarrierSlug => "dhl";

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

        var dhlRequest = new MockDhlRateRequest(
            new MockDhlParcel(
                WeightKg: request.Weight,
                SizeCm: new MockDhlDimensions(
                    Length: request.Length,
                    Width: request.Width,
                    Height: request.Height)));

        var dhlResponse = await mockDhlRatesClient.GetRatesAsync(endpoint.Endpoint, dhlRequest, cancellationToken);
        return dhlRateAdapter.Adapt(dhlResponse);
    }
}
