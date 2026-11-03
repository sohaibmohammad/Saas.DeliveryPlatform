using DeliveryPlatform.API.Common.Extensions;
using DeliveryPlatform.Application.src.Features.Inventory.GetByBranch;
using DeliveryPlatform.Application.src.Features.Inventory.ReserveStock;
using DeliveryPlatform.Application.src.Features.Inventory.SetInventory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryPlatform.API.Controllers;

[ApiController]
[Route("api/businesses/{businessId:guid}/branches/{branchId:guid}/inventory")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IMediator _mediator;

    public InventoryController(IMediator mediator)
    {
        _mediator = mediator;
    }


    [HttpPut("{productId:guid}")]
    public async Task<IActionResult> SetInventory(
     Guid businessId,
     Guid branchId,
     Guid productId,
     [FromBody] SetInventoryRequest request,
     CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var command = new SetInventoryCommand(
            userId,
            businessId,
            branchId,
            productId,
            request.Quantity);

        var inventoryId = await _mediator.Send(command, cancellationToken);

        return Ok(new { InventoryId = inventoryId });
    }

    [HttpGet]
    public async Task<IActionResult> GetByBranch(
        Guid businessId,
        Guid branchId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var query = new GetBranchInventoryQuery(
            businessId,
            branchId,
            userId,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveStock(
    Guid businessId,
    Guid branchId,
    Guid productId,
    [FromBody] ReserveStockRequest request,
    CancellationToken cancellationToken)
    {
        var command = new ReserveStockCommand(
            branchId,
            productId,
            request.QuantityToReserve);

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(new { Success = result });
    }
}
