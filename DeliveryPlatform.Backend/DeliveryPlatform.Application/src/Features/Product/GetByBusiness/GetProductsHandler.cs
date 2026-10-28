using AutoMapper;
using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using DeliveryPlatform.Application.src.Common.Paging;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Product.GetByBusiness;

public class GetProductsHandler : IRequestHandler<GetProductsQuery, PagedResult<ProductResponse>>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductsHandler(IBusinessRepository businessRepository, IProductRepository productRepository, IMapper mapper)
    {
        _businessRepository = businessRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<ProductResponse>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
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

        var products = await _productRepository.GetByBusinessIdAsync(request.BusinessId, request.CategoryId, cancellationToken);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var items = products
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => _mapper.Map<ProductResponse>(p))
            .ToList();

        return new PagedResult<ProductResponse>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = products.Count
        };
    }
}
