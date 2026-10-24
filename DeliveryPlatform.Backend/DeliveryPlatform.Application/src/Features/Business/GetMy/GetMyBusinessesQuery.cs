using DeliveryPlatform.Application.src.Common.Paging;
using DeliveryPlatform.Application.src.Features.Business.GetMy;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Business.GetMy;

public record GetMyBusinessesQuery(Guid UserId, int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<BusinessResponse>>;
