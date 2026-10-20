using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Auth.GoogleRegister
{
    public record RegisterWithGoogleCommand(string GoogleIdToken)
     : IRequest<string>;
}
