using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;

namespace DeliveryPlatform.Application.src.Features.Order.UpdateStatus;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, bool>
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

    public async Task<bool> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        if (request.OrderId == Guid.Empty)
            throw new InvalidOperationException("Invalid order ID.");

        var order = await _orderRepository.GetOrderForStatusUpdateAsync(
            request.OrderId,
            cancellationToken);

        if (order is null)
            throw new KeyNotFoundException("Order not found.");

        var userId = _currentUserService.UserId;
        if (order.Business?.UserId != userId)
            throw new UnauthorizedAccessException(
                "You are not authorized to update this order.");

        if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
            throw new InvalidOperationException(
                $"Order is already {order.Status} and cannot be updated.");

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

        if (request.Status == OrderStatus.Delivered)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async ct =>
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

                ApplyStatus(order, request.Status);
            }, cancellationToken);
        }
        else
        {
            ApplyStatus(order, request.Status);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return true;
    }

    private static void ApplyStatus(DeliveryPlatform.Domain.src.Entity.Order order, OrderStatus status)
    {
        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
    }
}
