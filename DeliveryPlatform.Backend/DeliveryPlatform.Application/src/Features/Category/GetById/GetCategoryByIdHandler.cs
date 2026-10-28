using AutoMapper;
using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Common.Exceptions;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Category.GetById;

public class GetCategoryByIdHandler : IRequestHandler<GetCategoryByIdQuery, CategoryByIdResponse?>
{
    private readonly IBusinessRepository _businessRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IMapper _mapper;

    public GetCategoryByIdHandler(IBusinessRepository businessRepository, ICategoryRepository categoryRepository, IMapper mapper)
    {
        _businessRepository = businessRepository;
        _categoryRepository = categoryRepository;
        _mapper = mapper;
    }

    public async Task<CategoryByIdResponse?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
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

        var category = await _categoryRepository.GetByBusinessIdAndCategoryIdAsync(
            request.BusinessId,
            request.CategoryId,
            cancellationToken);

        if (category is null)
            return null;

        return _mapper.Map<CategoryByIdResponse>(category);
    }
}
