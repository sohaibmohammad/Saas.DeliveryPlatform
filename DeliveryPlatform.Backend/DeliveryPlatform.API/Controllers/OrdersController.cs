using DeliveryPlatform.Application.src.Features.Order.Cancel;
using DeliveryPlatform.Application.src.Features.Order.Create;
using DeliveryPlatform.Application.src.Features.Order.Get;
using DeliveryPlatform.Application.src.Features.Order.GetBusinessOrders;
using DeliveryPlatform.Application.src.Features.Order.GetDetails;
using DeliveryPlatform.Application.src.Features.Order.UpdateStaus;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryPlatform.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrderCommand(
            request.BranchId,
            request.DeliveryAddress,
            request.Items);

        var result = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default)
    {
        var query = new GetMyOrdersQuery(pageNumber, pageSize);

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrderDetails(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetOrderDetailsQuery(orderId);

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    [HttpPost("{orderId}/cancel")]
    public async Task<IActionResult> CancelOrder(
        [FromRoute] Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var command = new CancelOrderCommand(orderId);

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }
    [HttpPut("{orderId:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(
    Guid orderId,
    [FromBody] UpdateOrderStatusRequest request,
    CancellationToken cancellationToken)
    {
        var command = new UpdaeOrderStatusCommand(
            orderId,
            request.Statuss
        );

        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
    [HttpGet("business/{businessId:guid}")]
    public async Task<IActionResult> GetBusinessOrders(
    [FromRoute] Guid businessId,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] OrderStatus? status = null,
    [FromQuery] Guid? branchId = null,
    CancellationToken cancellationToken = default)
    {
        var query = new GetBusinessOrdersQuery(
            businessId,
            pageNumber,
            pageSize,
            status,
            branchId
        );

        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}
