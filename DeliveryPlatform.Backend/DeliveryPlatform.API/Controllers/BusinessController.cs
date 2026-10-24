using DeliveryPlatform.Application.src.Features.Business.Create;
using DeliveryPlatform.Application.src.Features.Business.GetMy;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DeliveryPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessController : ControllerBase
{
    private readonly IMediator _mediator;

    public BusinessController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBusinessRequest request, CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var command = new CreateBusinessCommand(
            request.Name,
            request.Description,
            request.BusinessType,
            userId);

        var businessId = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new { id = businessId });
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMy([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        var result = await _mediator.Send(new GetMyBusinessesQuery(userId, pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }
}

public record CreateBusinessRequest(string Name, string? Description, Domain.src.Entity.BusinessType BusinessType);
