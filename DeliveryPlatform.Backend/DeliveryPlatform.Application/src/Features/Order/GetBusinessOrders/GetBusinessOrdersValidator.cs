using FluentValidation;

namespace DeliveryPlatform.Application.src.Features.Order.GetBusinessOrders;

public class GetBusinessOrdersValidator : AbstractValidator<GetBusinessOrdersQuery>
{
    public const int MaxPageSize = 50;

    public GetBusinessOrdersValidator()
    {
        RuleFor(x => x.BusinessId)
            .NotEmpty()
            .WithMessage("BusinessId is required.");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("PageNumber must be greater than zero.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("PageSize must be greater than zero.")
            .LessThanOrEqualTo(MaxPageSize)
            .WithMessage($"PageSize must not exceed {MaxPageSize}.");

        RuleFor(x => x.BranchId)
            .Must(branchId => !branchId.HasValue || branchId.Value != Guid.Empty)
            .WithMessage("BranchId must be a valid non-empty identifier.");
    }
}
