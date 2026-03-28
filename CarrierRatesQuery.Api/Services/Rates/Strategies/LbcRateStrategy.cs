using CarrierRatesQuery.Api.Data.Entities;
using CarrierRatesQuery.Api.Services.Rates.Clients;

namespace CarrierRatesQuery.Api.Services.Rates.Strategies;

public sealed class LbcRateStrategy(
    IMockLbcRatesClient mockLbcRatesClient,
    ICarrierRateAdapter<MockLbcRateResponse> lbcRateAdapter) : ICarrierRateStrategy
{
    public string CarrierSlug => "lbcexpress";

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

        var lbcRequest = new MockLbcRateRequest(
            new MockLbcPackage(
                Weight: request.Weight,
                Length: request.Length,
                Width: request.Width,
                Height: request.Height));

        var lbcResponse = await mockLbcRatesClient.GetRatesAsync(endpoint.Endpoint, lbcRequest, cancellationToken);
        return lbcRateAdapter.Adapt(lbcResponse);
    }
}