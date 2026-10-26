using DeliveryPlatform.API.Middleware;
using DeliveryPlatform.API.Options;
using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Application.src.Common.Behaviors;
using DeliveryPlatform.Application.src.Features.Auth.Register;
using DeliveryPlatform.Domain.src.Entity;
using DeliveryPlatform.Infrastructure.src.Database;
using DeliveryPlatform.Infrastructure.src.Repository;
using DeliveryPlatform.Infrastructure.src.Security;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddValidatorsFromAssembly(typeof(RegisterUserValidator).Assembly);

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblyContaining<RegisterUserHandler>();
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
}
);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    var connectionString = configuration.GetConnectionString(
        "RedisConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
        throw new InvalidOperationException(
            "Redis connection string is missing.");

    return ConnectionMultiplexer.Connect(connectionString);
});

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(option=>
option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services
    .AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.Issuer),
        "JWT Issuer is required.")
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.Audience),
        "JWT Audience is required.")
    .Validate(options =>
        !string.IsNullOrWhiteSpace(options.SecretKey)
        && System.Text.Encoding.UTF8.GetByteCount(options.SecretKey) >= 32,
        "JWT SecretKey must be at least 32 bytes.")
    .Validate(options =>
        options.ExpirationInMinutes > 0,
        "JWT expiration must be greater than zero.")
    .ValidateOnStart();



builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration
            .GetSection(JwtOptions.SectionName)
            .Get<JwtOptions>()!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents { 
            OnTokenValidated=async context =>
            {
                var services=context.HttpContext.RequestServices;
                var revocationService = services.GetRequiredService<ITokenRevocationService>();
                var userRepository = services.GetRequiredService<IUserRepository>();
                var jti=context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti).Value;
                var userIdValue=context.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? context.Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

                var tokenVersionValue = context.Principal?
                    .FindFirst("token_version")?.Value;
                if (string.IsNullOrWhiteSpace(jti) ||
                                 !Guid.TryParse(userIdValue, out var userId) ||
                                 !int.TryParse(tokenVersionValue, out var tokenVersion))
                {
                    context.Fail("Invalid token claims.");
                    return;
                }

                try
                {
                    if (await revocationService.IsRevokedAsync(jti))
                    {
                        context.Fail("Access token has been revoked.");
                        return;
                    }

                    var user = await userRepository.GetByIdAsync(
                        userId,
                        context.HttpContext.RequestAborted);

                    if (user is null ||
                        user.TokenVersion != tokenVersion)
                    {
                        context.Fail("Token is no longer valid.");
                        return;
                    }
                }
                catch (Exception)
                {
                    context.Fail("Unable to validate token.");
                }
            }
        };
    });



builder.Services
    .AddOptions<GoogleAuthOptions>()
    .Bind(builder.Configuration.GetSection(
        GoogleAuthOptions.SectionName))
    .Validate(
        options => !string.IsNullOrWhiteSpace(options.ClientId),
        "Google ClientId is required.")
    .ValidateOnStart();

builder.Services.AddScoped<IRefreshTokenService,RefreshTokenService>();
builder.Services.AddScoped<IUserRefreshTokenReposiory, UserRefreshTokenRepository>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IExternalLoginRepository,ExternalLoginRepository>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBusinessRepository, BusinessRepository>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPasswordHashService,PasswordHashService>();
builder.Services.AddScoped<ITokenRevocationService,RedisTokenRevocationService>();
builder.Services.AddScoped<IPasswordHasher<User>,PasswordHasher<User>>();



var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlingMiddlware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
