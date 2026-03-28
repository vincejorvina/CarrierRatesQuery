using FluentValidation;

namespace CarrierRatesQuery.Api.Services.Rates;

public sealed class RateQueryRequestValidator : AbstractValidator<RateQueryRequest>
{
    public RateQueryRequestValidator()
    {
        RuleFor(x => x.Weight)
            .GreaterThan(0m)
            .WithMessage("Weight must be greater than zero.");

        RuleFor(x => x.Length)
            .GreaterThan(0m)
            .WithMessage("Length must be greater than zero.");

        RuleFor(x => x.Width)
            .GreaterThan(0m)
            .WithMessage("Width must be greater than zero.");

        RuleFor(x => x.Height)
            .GreaterThan(0m)
            .WithMessage("Height must be greater than zero.");
    }
}
