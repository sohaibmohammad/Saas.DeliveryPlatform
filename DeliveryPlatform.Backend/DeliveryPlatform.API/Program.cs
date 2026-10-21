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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddValidatorsFromAssembly(typeof(RegisterUserValidator).Assembly);

builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssemblyContaining<RegisterUserHandler>();
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
}
);

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
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IPasswordHashService,PasswordHashService>();

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
