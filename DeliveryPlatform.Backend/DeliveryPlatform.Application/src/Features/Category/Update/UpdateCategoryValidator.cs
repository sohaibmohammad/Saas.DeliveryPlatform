using FluentValidation;

namespace DeliveryPlatform.Application.src.Features.Category.Update;

public class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.BusinessId)
            .NotEmpty();

        RuleFor(x => x.CategoryId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .MaximumLength(100);
    }
}
