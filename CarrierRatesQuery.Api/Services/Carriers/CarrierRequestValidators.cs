using FluentValidation;

namespace CarrierRatesQuery.Api.Services.Carriers;

public sealed class CreateCarrierRequestValidator : AbstractValidator<CreateCarrierRequest>
{
    public CreateCarrierRequestValidator()
    {
        CarrierValidationRules.Apply(RuleFor(x => x.Name));
    }
}

public sealed class UpdateCarrierRequestValidator : AbstractValidator<UpdateCarrierRequest>
{
    public UpdateCarrierRequestValidator()
    {
        CarrierValidationRules.Apply(RuleFor(x => x.Name));
    }
}

public sealed class DisableCarrierRequestValidator : AbstractValidator<DisableCarrierRequest>
{
    public DisableCarrierRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage("Disable reason is required.");
    }
}

internal static class CarrierValidationRules
{
    public static void Apply<T>(IRuleBuilderInitial<T, string> nameRule)
    {
        nameRule
            .NotEmpty()
            .WithMessage("Name is required.");
    }
}
