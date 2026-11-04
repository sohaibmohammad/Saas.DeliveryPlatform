using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Order.Create;

public class CreateOrderHandler:IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IBranchRepository _branchRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    private readonly IBusinessRepository _businessRepository;

    private readonly ICurrentUserService _currentUserService;

    public CreateOrderHandler(IBusinessRepository businessRepository,IBranchRepository branchRepository, IProductRepository productRepository, IInventoryRepository inventoryRepository, IOrderRepository orderRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _branchRepository = branchRepository;
        _productRepository = productRepository;
        _inventoryRepository = inventoryRepository;
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _businessRepository = businessRepository;
        _currentUserService = currentUserService;
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if(request.Items is null || !request.Items.Any())
        {
            throw new InvalidOperationException("Order must contain at least one item.");
        }

        if(string.IsNullOrWhiteSpace(request.DeliveryAddress))
        {
            throw new InvalidOperationException("Delivery address is required.");
        }

        if(request.Items.Any(i=>i.ProductId==Guid.Empty|| i.Quantity <= 0))
        {
            throw new InvalidOperationException("Invalid item or quantity.");
        }

        var branch = await _branchRepository.GetByIdAsync(request.BranchId, cancellationToken);
        if (branch is null|| !branch.IsActive)
        {
            throw new InvalidOperationException("Branch not found.");
        }
        var business = await _businessRepository.GetByIdAsync(
    branch.BusinessId,
    cancellationToken);

        if (business is null || !business.IsActive)
            throw new InvalidOperationException("Business not found or inactive.");

        var items=request.Items.GroupBy(i=>i.ProductId)
            .Select(g => new  
            {
                ProductId = g.Key,
                Quantity = g.Sum(i => i.Quantity)
            }).ToList();

        var productIds = items.Select(i => i.ProductId).ToList();

        var products = await _productRepository.GetByIdsAsync(
    business.Id,
    productIds,
    cancellationToken);

        if (products.Count != items.Count)
        {
            throw new InvalidOperationException(
                "One or more products were not found.");
        }

        if (products.Any(p => !p.IsAvailable))
        {
            throw new InvalidOperationException(
                "One or more products are unavailable.");
        }

        var productsById = products.ToDictionary(p => p.Id);

        decimal subTotal = items.Sum(item =>
            productsById[item.ProductId].Price * item.Quantity);
        DeliveryPlatform.Domain.src.Entity.Order order = null!;
        await _unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            foreach (var item in items)
            {
                var reserved = await _inventoryRepository.ReserveStockAsync(
                    request.BranchId,
                    item.ProductId,
                    item.Quantity,
                    ct);
                if (!reserved)
                {
                    throw new InvalidOperationException(
                        $"Insufficient stock for product {item.ProductId}.");
                }
            }

             order = new DeliveryPlatform.Domain.src.Entity.Order
            {
                Id = Guid.NewGuid(),
                UserId = _currentUserService.UserId, // إذا كان UserId موجوداً عندك في الـ Command
                BusinessId = business.Id,
                BranchId = branch.Id,
                OrderNumber = Guid.NewGuid().ToString("N"),
                Status = OrderStatus.Pending,
                Subtotal = subTotal,
                DeliveryFee = 0, // مؤقتاً إلى أن نحدد آلية التوصيل
                Total = subTotal,
                DeliveryAddress = request.DeliveryAddress.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            order.Items = items.Select(item =>
            {
                var product = productsById[item.ProductId];

                return new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    ProductName = product.Name,
                    UnitPrice = product.Price,
                    Quantity = (int)item.Quantity,
                    TotalPrice = product.Price * item.Quantity
                };
            }).ToList();
            await _orderRepository.AddAsync(order, ct);

        }, cancellationToken
        );

        return new CreateOrderResponse(
    order.Id,
    order.OrderNumber,
    order.Subtotal,
    order.DeliveryFee,
    order.Total,
    order.Status
);
    }
}
