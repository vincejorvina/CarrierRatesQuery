using System.Net.Http.Json;

namespace CarrierRatesQuery.Api.Services.Rates.Clients;

public interface IMockFedExRatesClient
{
    Task<MockFedExRateResponse> GetRatesAsync(string endpoint, MockFedExRateRequest request, CancellationToken cancellationToken);
}

public sealed class MockFedExRatesClient(HttpClient httpClient) : IMockFedExRatesClient
{
    public async Task<MockFedExRateResponse> GetRatesAsync(
        string endpoint,
        MockFedExRateRequest request,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new ArgumentException("Endpoint must be a valid absolute URL.", nameof(endpoint));
        }

        using var response = await httpClient.PostAsJsonAsync(endpointUri, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var rates = await response.Content.ReadFromJsonAsync<MockFedExRateResponse>(cancellationToken: cancellationToken);
        return rates ?? throw new HttpRequestException("FedEx rate response payload was empty.");
    }
}

public sealed record MockFedExRateRequest(MockFedExPackage Package);

public sealed record MockFedExPackage(decimal Weight, MockFedExDimensions Dimensions);

public sealed record MockFedExDimensions(decimal Length, decimal Width, decimal Height);

public sealed record MockFedExRateResponse(string Carrier, IReadOnlyList<MockFedExServiceOption> ServiceOptions);

public sealed record MockFedExServiceOption(string ServiceName, string EstimatedDelivery, decimal Rate);
