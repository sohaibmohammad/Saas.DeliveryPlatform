using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Inventory.ReserveStock;

public record ReserveStockCommand(Guid BranchId, Guid ProductId, int QuantityToReserve) : IRequest<bool>;
