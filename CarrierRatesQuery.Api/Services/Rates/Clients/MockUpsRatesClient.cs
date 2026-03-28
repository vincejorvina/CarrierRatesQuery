using System.Net.Http.Json;

namespace CarrierRatesQuery.Api.Services.Rates.Clients;

public interface IMockUpsRatesClient
{
    Task<MockUpsRateResponse> GetRatesAsync(string endpoint, MockUpsRateRequest request, CancellationToken cancellationToken);
}

public sealed class MockUpsRatesClient(HttpClient httpClient) : IMockUpsRatesClient
{
    public async Task<MockUpsRateResponse> GetRatesAsync(
        string endpoint,
        MockUpsRateRequest request,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new ArgumentException("Endpoint must be a valid absolute URL.", nameof(endpoint));
        }

        using var response = await httpClient.PostAsJsonAsync(endpointUri, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var rates = await response.Content.ReadFromJsonAsync<MockUpsRateResponse>(cancellationToken: cancellationToken);
        return rates ?? throw new HttpRequestException("UPS rate response payload was empty.");
    }
}

public sealed record MockUpsRateRequest(MockUpsShipment Shipment);

public sealed record MockUpsShipment(decimal WeightLbs, MockUpsDimensions DimensionsInches);

public sealed record MockUpsDimensions(decimal Length, decimal Width, decimal Height);

public sealed record MockUpsRateResponse(string Company, IReadOnlyList<MockUpsServiceOption> Services);

public sealed record MockUpsServiceOption(string Service, string Eta, decimal Cost);
