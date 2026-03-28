using FluentValidation;

namespace CarrierRatesQuery.Api.Services.CarrierEndpoints;

public sealed class CreateCarrierEndpointRequestValidator : AbstractValidator<CreateCarrierEndpointRequest>
{
    public CreateCarrierEndpointRequestValidator()
    {
        CarrierEndpointValidationRules.Apply(this);
    }
}

public sealed class UpdateCarrierEndpointRequestValidator : AbstractValidator<UpdateCarrierEndpointRequest>
{
    public UpdateCarrierEndpointRequestValidator()
    {
        CarrierEndpointValidationRules.Apply(this);
    }
}

internal static class CarrierEndpointValidationRules
{
    public static void Apply(AbstractValidator<CreateCarrierEndpointRequest> validator)
    {
        validator.RuleFor(x => x.Operation)
            .NotEmpty()
            .WithMessage("Operation is required.");

        validator.RuleFor(x => x.Endpoint)
            .NotEmpty()
            .WithMessage("Endpoint URL is required.");
    }

    public static void Apply(AbstractValidator<UpdateCarrierEndpointRequest> validator)
    {
        validator.RuleFor(x => x.Operation)
            .NotEmpty()
            .WithMessage("Operation is required.");

        validator.RuleFor(x => x.Endpoint)
            .NotEmpty()
            .WithMessage("Endpoint URL is required.");
    }
}
