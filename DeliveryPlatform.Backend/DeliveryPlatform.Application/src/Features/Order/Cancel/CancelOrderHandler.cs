using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Order.Cancel;

public class CancelOrderHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly ICurrentUserService _currentUserService;
    public CancelOrderHandler(IInventoryRepository inventoryRepository, IOrderRepository orderRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _inventoryRepository = inventoryRepository;
    }
    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
            throw new InvalidOperationException("Invalid order ID.");
        var userId = _currentUserService.UserId;
        var order =await _orderRepository.GetForCancellationAsync(request.OrderId, userId, cancellationToken);
        
        if (order is null) 
            throw new KeyNotFoundException("Order not found.");
        if (!order.Status.Equals(OrderStatus.Pending))
            throw new InvalidOperationException(
                "Only pending orders can be cancelled.");
        
        await _unitOfWork.ExecuteInTransactionAsync(async ct => { 
        foreach(var item in order.Items)
            {
                var released = await _inventoryRepository.ReleaseReservedStockAsync(order.BranchId, item.ProductId, item.Quantity,ct);
                if (!released)
                {
                    throw new InvalidOperationException(
                        $"Failed to release stock for product {item.ProductId}.");
                }
            }
            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
        
        }, cancellationToken);

        return true;


    }
}
