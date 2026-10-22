using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Auth.Logout;

public record LogoutCommand(string RefreshToken):IRequest
{
}
