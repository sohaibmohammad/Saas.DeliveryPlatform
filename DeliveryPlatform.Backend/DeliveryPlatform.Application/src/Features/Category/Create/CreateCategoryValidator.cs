using FluentValidation;

namespace DeliveryPlatform.Application.src.Features.Category.Create;

public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.BusinessId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .MaximumLength(100);
    }
}
