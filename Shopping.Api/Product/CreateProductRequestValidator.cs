using FluentValidation;
using FluentValidation.Results;

namespace Shopping.Api.Controllers;

public class CreateProductRequestValidator :AbstractValidator<CreateProductRequest>
{
    public ProductValidation()
    {
        RuleFor(x => x.Description)
            .NotEmpty();

        RuleFor(x => x.Sku)
            .NotEmpty();

        RuleFor(x => x.Cost)
            .NotEmpty();
    }
}