using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Auth.Refresh;

public record RefreshTokenCommand(string RefreshToken):IRequest<RefreshTokenResponse>;
