using DeliveryPlatform.API.Common.Extensions;
using DeliveryPlatform.Application.src.Features.Branch.Create;
using DeliveryPlatform.Application.src.Features.Branch.GetByBusiness;
using DeliveryPlatform.Application.src.Features.Branch.GetById;
using DeliveryPlatform.Application.src.Features.Branch.Update;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryPlatform.API.Controllers;

[ApiController]
[Route("api/businesses/{businessId:guid}/branches")]
[Authorize]
public class BranchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BranchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid businessId, [FromBody] CreateBranchRequest request, CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var command = new CreateBranchCommand(
            businessId,
            request.Name,
            request.Address,
            request.Latitude,
            request.Longitude,
            userId);

        var branchId = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new { id = branchId });
    }

    [HttpPut("{branchId:guid}")]
    public async Task<IActionResult> Update(
        Guid businessId,
        Guid branchId,
        [FromBody] UpdateBranchRequest request,
        CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var command = new UpdateBranchCommand(
            businessId,
            branchId,
            request.Name,
            request.Address,
            request.Latitude,
            request.Longitude,
            request.IsActive,
            userId);

        var updatedBranchId = await _mediator.Send(command, cancellationToken);
        return Ok(new { id = updatedBranchId });
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

        var query = new GetBranchesQuery(
            businessId,
            userId,
            User.IsInRole(UserRole.Admin.ToString()),
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{branchId:guid}")]
    public async Task<IActionResult> GetById(
        Guid businessId,
        Guid branchId,
        CancellationToken cancellationToken)
    {
        if (User.GetUserId() is not { } userId)
            return Unauthorized();

        var query = new GetBranchByIdQuery(
            businessId,
            branchId,
            userId,
            User.IsInRole(UserRole.Admin.ToString()));

        var result = await _mediator.Send(query, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }
}
