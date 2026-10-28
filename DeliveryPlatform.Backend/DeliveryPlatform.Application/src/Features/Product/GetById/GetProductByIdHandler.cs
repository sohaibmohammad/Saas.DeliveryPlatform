using AutoMapper;
using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Product.GetById;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductByIdResponse?>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetProductByIdHandler(IBusinessRepository businessRepository, IProductRepository productRepository, IMapper mapper)
    {
        _businessRepository = businessRepository;
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<ProductByIdResponse?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
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

        var product = await _productRepository.GetByBusinessIdAndProductIdAsync(
            request.BusinessId,
            request.ProductId,
            cancellationToken);

        if (product is null)
            return null;

        return _mapper.Map<ProductByIdResponse>(product);
    }
}
