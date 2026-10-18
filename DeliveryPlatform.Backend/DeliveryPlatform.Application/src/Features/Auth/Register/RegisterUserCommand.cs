using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Auth.Register;

public record RegisterUserCommand(
    string Name,
    string Email,
    string Password
) : IRequest<Guid>;
