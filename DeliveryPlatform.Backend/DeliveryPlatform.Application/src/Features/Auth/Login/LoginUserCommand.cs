using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Auth.Login;

public record LoginUserCommand(
string Email,
string Password
) : IRequest<Guid>;
