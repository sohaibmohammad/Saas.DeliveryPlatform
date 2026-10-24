using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Paging;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Business.GetMy;

public class GetMyBusinessesHandler : IRequestHandler<GetMyBusinessesQuery, PagedResult<BusinessResponse>>
{
    private readonly IBusinessRepository _businessRepository;

    public GetMyBusinessesHandler(IBusinessRepository businessRepository)
    {
        _businessRepository = businessRepository;
    }

    public async Task<PagedResult<BusinessResponse>> Handle(GetMyBusinessesQuery request, CancellationToken cancellationToken)
    {
        var businesses = await _businessRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var totalCount = businesses.Count;

        var items = businesses
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BusinessResponse(
                b.Id,
                b.Name,
                b.Description,
                b.BusinessType,
                b.IsActive,
                b.CreatedAt
            ))
            .ToList();

        return new PagedResult<BusinessResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
