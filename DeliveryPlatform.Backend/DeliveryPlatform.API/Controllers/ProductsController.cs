using DeliveryPlatform.API.Common.Extensions;
using DeliveryPlatform.Application.src.Features.Product.Create;
using DeliveryPlatform.Application.src.Features.Product.GetByBusiness;
using DeliveryPlatform.Application.src.Features.Product.GetById;
using DeliveryPlatform.Application.src.Features.Product.Update;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryPlatform.API.Controllers;

[ApiController]
[Route("api/businesses/{businessId:guid}/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid businessId, [FromBody] CreateProductRequest request, CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var command = new CreateProductCommand(
            businessId,
            request.CategoryId,
            request.Name,
            request.Description,
            request.Price,
            request.IsAvailable,
            userId);

        var productId = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new { id = productId });
    }

    [HttpPut("{productId:guid}")]
    public async Task<IActionResult> Update(Guid businessId, Guid productId, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var command = new UpdateProductCommand(
            businessId,
            productId,
            request.CategoryId,
            request.Name,
            request.Description,
            request.Price,
            request.IsAvailable,
            userId);

        var updatedProductId = await _mediator.Send(command, cancellationToken);
        return Ok(new { id = updatedProductId });
    }

    [HttpGet]
    public async Task<IActionResult> GetByBusiness(
        Guid businessId,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var query = new GetProductsQuery(
            businessId,
            userId,
            User.IsInRole(UserRole.Admin.ToString()),
            categoryId,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{productId:guid}")]
    public async Task<IActionResult> GetById(Guid businessId, Guid productId, CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var query = new GetProductByIdQuery(
            businessId,
            productId,
            userId,
            User.IsInRole(UserRole.Admin.ToString()));

        var result = await _mediator.Send(query, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
