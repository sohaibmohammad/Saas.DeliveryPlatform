using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Order.UpdateStaus;

public class UpdateOrderStatusHandler : IRequestHandler<UpdaeOrderStatusCommand, bool>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IInventoryRepository _inventoryRepository;

    public UpdateOrderStatusHandler(
        IInventoryRepository inventoryRepository,
        IOrderRepository orderRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
        _inventoryRepository = inventoryRepository;
        
    }

    public async Task<bool> Handle(UpdaeOrderStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
            throw new InvalidOperationException("Invalid order ID.");

        var order = await _orderRepository.GetOrderForSatusUpdateAsync(request.OrderId,cancellationToken);
        if (order is null)
            throw new KeyNotFoundException("Order not found");
        var userId = _currentUserService.UserId;
        if (order.Business.UserId != userId)
            throw new UnauthorizedAccessException(
                "You are not authorized to update this order.");

        var isValidTransition = (order.Status, request.Status) switch
        {
            (OrderStatus.Pending, OrderStatus.Confirmed) => true,
            (OrderStatus.Confirmed, OrderStatus.Preparing) => true,
            (OrderStatus.Preparing, OrderStatus.ReadyForPickup) => true,
            (OrderStatus.ReadyForPickup, OrderStatus.OutForDelivery) => true,
            (OrderStatus.OutForDelivery, OrderStatus.Delivered) => true,
            _ => false
        };

        if (!isValidTransition)
            throw new InvalidOperationException(
                $"Cannot change order status from {order.Status} to {request.Status}.");

        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            if (request.Status == OrderStatus.Delivered)
            {
                foreach (var item in order.Items)
                {
                    var committed = await _inventoryRepository.CommitReservedStockAsync(
                        order.BranchId,
                        item.ProductId,
                        item.Quantity,
                        ct);

                    if (!committed)
                    {
                        throw new InvalidOperationException(
                            $"Failed to commit stock for product {item.ProductId}.");
                    }
                }
            }

            order.Status = request.Status;
            order.UpdatedAt = DateTime.UtcNow;

        }, cancellationToken);

        return true;
    }
}
