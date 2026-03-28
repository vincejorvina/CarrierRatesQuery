using System.Net.Http.Json;

namespace CarrierRatesQuery.Api.Services.Rates.Clients;

public interface IMockLbcRatesClient
{
    Task<MockLbcRateResponse> GetRatesAsync(string endpoint, MockLbcRateRequest request, CancellationToken cancellationToken);
}

public sealed class MockLbcRatesClient(HttpClient httpClient) : IMockLbcRatesClient
{
    public async Task<MockLbcRateResponse> GetRatesAsync(
        string endpoint,
        MockLbcRateRequest request,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new ArgumentException("Endpoint must be a valid absolute URL.", nameof(endpoint));
        }

        using var response = await httpClient.PostAsJsonAsync(endpointUri, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var rates = await response.Content.ReadFromJsonAsync<MockLbcRateResponse>(cancellationToken: cancellationToken);
        return rates ?? throw new HttpRequestException("LBC rate response payload was empty.");
    }
}

public sealed record MockLbcRateRequest(MockLbcPackage Package);

public sealed record MockLbcPackage(decimal Weight, decimal Length, decimal Width, decimal Height);

public sealed record MockLbcRateResponse(string Carrier, IReadOnlyList<MockLbcServiceOption> ServiceOptions);

public sealed record MockLbcServiceOption(string ServiceName, string EstimatedDelivery, decimal Rate);