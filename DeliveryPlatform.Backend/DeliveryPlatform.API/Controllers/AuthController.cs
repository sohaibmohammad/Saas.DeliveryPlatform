using DeliveryPlatform.Application.src.Features.Auth.GoogleRegister;
using DeliveryPlatform.Application.src.Features.Auth.Login;
using DeliveryPlatform.Application.src.Features.Auth.LoginWithGoogle.cs;
using DeliveryPlatform.Application.src.Features.Auth.Logout;
using DeliveryPlatform.Application.src.Features.Auth.LogoutAllDvices;
using DeliveryPlatform.Application.src.Features.Auth.Refresh;
using DeliveryPlatform.Application.src.Features.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DeliveryPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController:ControllerBase
{
    private readonly IMediator _mediator;
    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var userId = await _mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new { id = userId });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        var token = await _mediator.Send(command, cancellationToken);

        return Ok(new { tokens=token  });
    }

    [HttpPost("google/register")]
    public async Task<IActionResult> RegisterWithGoogle(
   [FromBody] RegisterWithGoogleCommand command,
   CancellationToken cancellationToken)
    {
        var token = await _mediator.Send(command, cancellationToken);

        return Ok(new { token });
    }

    [HttpPost("google/login")]
    public async Task<IActionResult> LoginWithGoogle(
        LoginWithGoogleCommand  command,
        CancellationToken cancellationToken)
    {
        var token = await _mediator.Send(command, cancellationToken);

        return Ok(new { tokens=token });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenResponse>> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }


    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutCommand  command,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(command, cancellationToken);

        return NoContent();
    }
    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAllDevices(
CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdValue, out var userId))
            return Unauthorized();

        await _mediator.Send(
            new LogoutAllDevicesCommand(userId),
            cancellationToken);

        return NoContent();
    }


}
