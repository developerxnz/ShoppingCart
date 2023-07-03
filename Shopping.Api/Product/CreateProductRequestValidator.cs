using FluentValidation;

namespace Shopping.Api.Controllers;

public class CreateProductRequestValidator :AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleSet("standard", () =>
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage($"{nameof(CreateProductRequest.Description)} is required.");

            RuleFor(x => x.Sku)
                .NotEmpty()
                .WithMessage($"{nameof(CreateProductRequest.Sku)} is required.");
            
            RuleFor(x => x.Cost)
                .NotEmpty()
                .WithMessage($"{nameof(CreateProductRequest.Cost)} is required.");
        });
    }
}