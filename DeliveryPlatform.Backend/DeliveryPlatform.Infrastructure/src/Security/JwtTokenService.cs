using DeliveryPlatform.API.Options;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Domain.src.Entity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeliveryPlatform.Infrastructure.src.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _jwtOptions;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
    }
    public string GenerateToken(User user)
    {
        throw new NotImplementedException();
    }
}
