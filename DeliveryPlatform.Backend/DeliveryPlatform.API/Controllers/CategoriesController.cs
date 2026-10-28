using DeliveryPlatform.API.Common.Extensions;
using DeliveryPlatform.Application.src.Features.Category.Create;
using DeliveryPlatform.Application.src.Features.Category.GetByBusiness;
using DeliveryPlatform.Application.src.Features.Category.GetById;
using DeliveryPlatform.Application.src.Features.Category.Update;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryPlatform.API.Controllers;

[ApiController]
[Route("api/businesses/{businessId:guid}/categories")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid businessId, [FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var command = new CreateCategoryCommand(businessId, request.Name, userId);
        var categoryId = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new { id = categoryId });
    }

    [HttpPut("{categoryId:guid}")]
    public async Task<IActionResult> Update(Guid businessId, Guid categoryId, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var command = new UpdateCategoryCommand(businessId, categoryId, request.Name, userId);
        var updatedCategoryId = await _mediator.Send(command, cancellationToken);
        return Ok(new { id = updatedCategoryId });
    }

    [HttpGet]
    public async Task<IActionResult> GetByBusiness(
        Guid businessId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var query = new GetCategoriesQuery(
            businessId,
            userId,
            User.IsInRole(UserRole.Admin.ToString()),
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{categoryId:guid}")]
    public async Task<IActionResult> GetById(Guid businessId, Guid categoryId, CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var query = new GetCategoryByIdQuery(
            businessId,
            categoryId,
            userId,
            User.IsInRole(UserRole.Admin.ToString()));

        var result = await _mediator.Send(query, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
