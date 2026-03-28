# CarrierRatesQuery

Carrier rates backend assessment project built with .NET 8.

The system aggregates shipping rates from multiple mock carrier APIs and exposes a unified rates API, plus carrier and endpoint management APIs.

## Tech Stack

- .NET 8 Web API
- EF Core InMemory database
- Aspire AppHost for local orchestration
- xUnit for unit tests
- Swagger/OpenAPI for API docs

## Solution Structure

- `CarrierRatesQuery.Api` - main API (rate aggregation + carrier/endpoint management)
- `CarrierRatesQuery.AppHost` - Aspire host to run all services locally
- `CarrierRatesQuery.MockFedEx` - mock FedEx API
- `CarrierRatesQuery.MockUps` - mock UPS API
- `CarrierRatesQuery.MockDhl` - mock DHL API
- `CarrierRatesQuery.MockLbc` - extra local mock carrier for demo purposes
- `CarrierRatesQuery.Tests` - unit tests

## High-Level Architecture

### Main Flow

1. Client sends rate query to `CarrierRatesQuery.Api`.
2. API loads enabled carriers/endpoints from InMemory DB.
3. API selects the correct carrier integration via Strategy pattern (`carrier slug -> strategy`).
4. Strategy calls the corresponding mock carrier client (`HttpClientFactory` typed clients).
5. Adapter normalizes carrier-specific response to unified response DTO.
6. API returns aggregated rates.

### Design Choices

- **Strategy Pattern**: one strategy per carrier (`fedex`, `ups`, `dhl`) for extensibility.
- **Adapter Pattern**: one adapter per carrier response format mapped into a single DTO shape.
- **Open/Closed**: add a new carrier by adding a new client + adapter + strategy + DI registration (minimal changes to core flow).

### Implemented Runtime Features

- **In-memory rate caching**: rate query results are cached in-memory per carrier/query key to reduce redundant outbound carrier calls.
- **Retry & resilience**: carrier API calls use `Microsoft.Extensions.Http.Resilience` standard resilience handler (retry, circuit breaker, timeout).
- **Role-aware disable flow via header**: `X-Role` controls admin-restricted actions; non-admin direct disable is blocked.
- **Disable request workflow**: users can submit carrier disable requests; admins can approve/reject requests.

## Running Locally

### Prerequisites

- .NET 8 SDK (or newer SDK that can target `net8.0`)

### Option A (Recommended): Run with Aspire AppHost

From repository root:

```bash
dotnet run --project CarrierRatesQuery.AppHost
```

This starts:

- main API
- all mock carrier APIs
- Aspire dashboard (with service endpoints)

Use the dashboard to open each service's Swagger UI.

### Option B: Run services manually

Open separate terminals and run:

```bash
dotnet run --project CarrierRatesQuery.MockFedEx
dotnet run --project CarrierRatesQuery.MockUps
dotnet run --project CarrierRatesQuery.MockDhl
dotnet run --project CarrierRatesQuery.MockLbc
dotnet run --project CarrierRatesQuery.Api
```

Then open Swagger/API URLs from terminal output.

## Testing

Run unit tests:

```bash
dotnet test CarrierRatesQuery.Tests/CarrierRatesQuery.Tests.csproj
```

## Bonus Features

- **Frontend Admin Tool**: A Vue/Vite admin dashboard that interacts with the API to demonstrate full-stack capabilities (rate querying, carrier management, disable request workflow).

## Notes

- Database is seeded in-memory on startup (`CarrierRatesQuery.Api`).
- Carrier API HTTP clients use standard resilience (retry, circuit breaker, timeout) via `Microsoft.Extensions.Http.Resilience`.
