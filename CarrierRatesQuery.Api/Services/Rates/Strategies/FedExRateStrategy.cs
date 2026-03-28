using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.Rates.Clients;

namespace CarrierRatesQuery.Api.Services.Rates.Strategies;

public sealed class FedExRateStrategy(
    IMockFedExRatesClient mockFedExRatesClient,
    ICarrierRateAdapter<MockFedExRateResponse> fedExRateAdapter) : ICarrierRateStrategy
{
    public string CarrierSlug => "fedex";

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

        var fedExRequest = new MockFedExRateRequest(
            new MockFedExPackage(
                Weight: request.Weight,
                Dimensions: new MockFedExDimensions(
                    Length: request.Length,
                    Width: request.Width,
                    Height: request.Height)));

        var fedExResponse = await mockFedExRatesClient.GetRatesAsync(endpoint.Endpoint, fedExRequest, cancellationToken);
        return fedExRateAdapter.Adapt(fedExResponse);
    }
}
