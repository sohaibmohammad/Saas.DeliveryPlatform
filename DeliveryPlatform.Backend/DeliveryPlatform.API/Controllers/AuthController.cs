using DeliveryPlatform.Application.src.Features.Auth.GoogleRegister;
using DeliveryPlatform.Application.src.Features.Auth.Login;
using DeliveryPlatform.Application.src.Features.Auth.LoginWithGoogle.cs;
using DeliveryPlatform.Application.src.Features.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace DeliveryPlatform.API.Controllers
{
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

            return Ok(new { AccessToken=token ,TokenType="Bearer" });
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

            return Ok(new { AccessToken = token, TokenType = "Bearer" });
        }
    }
}
