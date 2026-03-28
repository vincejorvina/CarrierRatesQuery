namespace CarrierRatesQuery.Api.Services.Rates.Strategies;

public interface ICarrierRateStrategyResolver
{
    bool TryResolve(string carrierSlug, out ICarrierRateStrategy strategy);
}

public sealed class CarrierRateStrategyResolver(IEnumerable<ICarrierRateStrategy> strategies) : ICarrierRateStrategyResolver
{
    private readonly Dictionary<string, ICarrierRateStrategy> strategyMap = strategies
        .GroupBy(x => x.CarrierSlug, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

    public bool TryResolve(string carrierSlug, out ICarrierRateStrategy strategy)
    {
        return strategyMap.TryGetValue(carrierSlug, out strategy!);
    }
}
