using AutoMapper;
using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using DeliveryPlatform.Application.src.Common.Paging;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Category.GetByBusiness;

public class GetCategoriesHandler : IRequestHandler<GetCategoriesQuery, PagedResult<CategoryResponse>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetCategoriesHandler(IBusinessRepository businessRepository, ICategoryRepository categoryRepository, IMapper mapper)
    {
        _businessRepository = businessRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<CategoryResponse>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        if (!request.IsAdmin)
        {
            var business = await _businessRepository.GetByUserIdAndBusinessIdAsync(
                request.UserId,
                request.BusinessId,
                cancellationToken);

            if (business is null)
                throw new NotFoundException("Business not found.");
        }

        var categories = await _categoryRepository.GetByBusinessIdAsync(request.BusinessId, cancellationToken);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var items = categories
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(c => _mapper.Map<CategoryResponse>(c))
            .ToList();

        return new PagedResult<CategoryResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = categories.Count
        };
    }
}
