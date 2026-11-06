using DeliveryPlatform.Application.src.Features.Order.Cancel;
using DeliveryPlatform.Application.src.Features.Order.Create;
using DeliveryPlatform.Application.src.Features.Order.Get;
using DeliveryPlatform.Application.src.Features.Order.GetDetails;
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
}
