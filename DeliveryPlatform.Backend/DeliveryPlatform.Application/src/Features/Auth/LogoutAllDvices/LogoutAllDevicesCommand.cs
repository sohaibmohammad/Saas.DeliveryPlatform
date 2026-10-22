using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Auth.LogoutAllDvices;

public record LogoutAllDevicesCommand(Guid UserId) : IRequest;
