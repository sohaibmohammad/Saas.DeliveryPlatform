using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Inventory.ReserveStock;

public class ReserveStockHandler : IRequestHandler<ReserveStockCommand, bool>
{
    private readonly IInventoryRepository _inventoryRepository;

    public ReserveStockHandler(IInventoryRepository inventoryRepository)
    {
        _inventoryRepository = inventoryRepository;
    }
    public async Task<bool> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        if (request.QuantityToReserve <= 0)
        {
            throw new ArgumentException("Quantity to reserve must be greater than zero.");
        }

        var reserved= await _inventoryRepository.ReserveStockAsync(request.BranchId, request.ProductId, request.QuantityToReserve, cancellationToken);
    
        if(!reserved)
        {
            throw new InvalidOperationException("Insufficient stock available to reserve.");
        }
        return true;
    }
}
