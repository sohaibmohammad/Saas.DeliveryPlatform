using DeliveryPlatform.Application.src.Features.Auth.Login;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Application.src.Features.Auth.LoginWithGoogle.cs;

public record LoginWithGoogleCommand(
 string GoogleIdToken
) : IRequest<LoginUserResponse>;
