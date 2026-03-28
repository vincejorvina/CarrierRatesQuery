using System.Net.Http.Json;

namespace CarrierRatesQuery.Api.Services.Rates.Clients;

public interface IMockDhlRatesClient
{
    Task<MockDhlRateResponse> GetRatesAsync(string endpoint, MockDhlRateRequest request, CancellationToken cancellationToken);
}

public sealed class MockDhlRatesClient(HttpClient httpClient) : IMockDhlRatesClient
{
    public async Task<MockDhlRateResponse> GetRatesAsync(
        string endpoint,
        MockDhlRateRequest request,
        CancellationToken cancellationToken)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var endpointUri))
        {
            throw new ArgumentException("Endpoint must be a valid absolute URL.", nameof(endpoint));
        }

        using var response = await httpClient.PostAsJsonAsync(endpointUri, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var rates = await response.Content.ReadFromJsonAsync<MockDhlRateResponse>(cancellationToken: cancellationToken);
        return rates ?? throw new HttpRequestException("DHL rate response payload was empty.");
    }
}

public sealed record MockDhlRateRequest(MockDhlParcel Parcel);

public sealed record MockDhlParcel(decimal WeightKg, MockDhlDimensions SizeCm);

public sealed record MockDhlDimensions(decimal Length, decimal Width, decimal Height);

public sealed record MockDhlRateResponse(string Provider, IReadOnlyList<MockDhlServiceOption> Options);

public sealed record MockDhlServiceOption(string Name, string DeliveryDate, decimal Price);
