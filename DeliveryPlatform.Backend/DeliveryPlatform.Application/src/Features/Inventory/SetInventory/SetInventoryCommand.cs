using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Inventory.SetInventory;

public record SetInventoryCommand(
Guid UserId,
Guid BusinessId,
Guid BranchId,
Guid ProductId,
int Quantity
) : IRequest<Guid>;
