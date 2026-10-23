using Azure.Core;
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
using System.IdentityModel.Tokens.Jwt;
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

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody]  LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var exp = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

        if (string.IsNullOrWhiteSpace(jti) ||
            !Guid.TryParse(userId, out var parsedUserId) ||
            !long.TryParse(exp, out var expiration))
        {
            return Unauthorized();
        }

        var expiresAt = DateTimeOffset
            .FromUnixTimeSeconds(expiration)
            .UtcDateTime;

        await _mediator.Send(
            new LogoutCommand(
                request.RefreshToken,
                jti,
                expiresAt,
                parsedUserId),
            cancellationToken);

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
