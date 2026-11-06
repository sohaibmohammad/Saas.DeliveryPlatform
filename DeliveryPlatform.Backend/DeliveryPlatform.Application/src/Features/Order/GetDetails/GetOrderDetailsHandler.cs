using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Order.GetDetails;

public class GetOrderDetailsHandler : IRequestHandler<GetOrderDetailsQuery, OrderDetailsResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetOrderDetailsHandler(
        IOrderRepository orderRepository,
        ICurrentUserService currentUserService)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
    }
    public async Task<OrderDetailsResponse> Handle(GetOrderDetailsQuery request, CancellationToken cancellationToken)
    {
        if(request.OrderId == Guid.Empty)
        {
            throw new InvalidOperationException("Invalid order ID.");
        }
        var userId = _currentUserService.UserId;
        var order = await _orderRepository.GetOrderDetailsAsync(request.OrderId, userId, cancellationToken);
        
        if (order == null)
        {
            throw new InvalidOperationException("Order not found.");
        }

        return new OrderDetailsResponse(
                  order.Id,
                  order.OrderNumber,
                  order.Status,
                  order.Business.Name,
                  order.Branch.Name,
                  order.DeliveryAddress,
                  order.Subtotal,
                  order.DeliveryFee,
                  order.Total,
                  order.CreatedAt,
                  order.Items.Select(item => new OrderItemDetailsResponse(
                      item.ProductId,
                      item.ProductName,
                      item.UnitPrice,
                      item.Quantity,
                      item.TotalPrice
                  )).ToList()
              );
    }
}
