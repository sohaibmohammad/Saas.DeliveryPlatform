using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Inventory.SetInventory;

public class SetInventoryHandler:IRequestHandler<SetInventoryCommand,Guid>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IBranchRepository _branchRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    public SetInventoryHandler(
      IInventoryRepository inventoryRepository,
      IBusinessRepository businessRepository,
      IBranchRepository branchRepository,
      IProductRepository productRepository,
      IUnitOfWork unitOfWork)
    {
        _inventoryRepository = inventoryRepository;
        _businessRepository = businessRepository;
        _branchRepository = branchRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(SetInventoryCommand request, CancellationToken cancellationToken)
    {
        if (request.Quantity < 0)
        {
            throw new ArgumentException("Quantity cannot be negative.");
        }
        var business=await _businessRepository.GetByIdAsync(request.BusinessId,cancellationToken);
        if(business == null)
        {
            throw new ArgumentException("Business not found.");
        }
        if (business.UserId != request.UserId)
        {
            throw new UnauthorizedAccessException("User does not have permission to set inventory for this business.");
        }

        var branch = await _branchRepository.GetByIdAsync(request.BranchId, cancellationToken);
        if (branch == null || branch.BusinessId != request.BusinessId)
        {
            throw new ArgumentException("Branch not found or does not belong to the specified business.");
        }
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null || product.BusinessId != request.BusinessId)
            throw new KeyNotFoundException(
                "Product not found in this business");

        var inventory = await _inventoryRepository.GetByBranchIdAndProductIdAsync(request.BranchId, request.ProductId, cancellationToken);

        if (inventory is null)
        {
            inventory = new DeliveryPlatform.Domain.src.Entity.Inventory
            {
                Id = Guid.NewGuid(),
                BranchId = request.BranchId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                ReservedQuantity = 0,
                UpdatedAt = DateTime.UtcNow
            };

            await _inventoryRepository.AddAsync(inventory, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return inventory.Id;
        }
        var updated=await _inventoryRepository.SetQuantityAsync(inventory.Id, request.Quantity, cancellationToken);
        if (!updated)
        {
            throw new InvalidOperationException("Cannot set quantity below reserved amount.");
        }
        return inventory.Id;
    }
}
