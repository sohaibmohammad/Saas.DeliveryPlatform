using DeliveryPlatform.Application.src.Abstractions.Repositoty;
using DeliveryPlatform.Application.src.Abstractions.Security;
using DeliveryPlatform.Domain.src.Entity;
using MediatR;
 

namespace DeliveryPlatform.Application.src.Features.Auth.Login;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, LoginUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IUserRefreshTokenReposiory _refreshTokenRepository;

    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;


    public LoginUserHandler(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository,
        IPasswordHashService passwordHashService, IJwtTokenService jwtTokenService, IRefreshTokenService refreshTokenService
       , IUserRefreshTokenReposiory refreshTokenRepository)
    {
        _userRepository = userRepository;
        _passwordHashService = passwordHashService;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;

        _unitOfWork = unitOfWork;


    }
    public async Task<LoginUserResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _userRepository.GetByEmailAsync(
            email,
            cancellationToken);

        if (user is null||user.PasswordHash is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var isPasswordValid = _passwordHashService.VerifyPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var accessToken= _jwtTokenService.GenerateToken(user);
        var refreshToken =
          _refreshTokenService.GenerateRefreshToken();
        var refreshTokenHash =
            _refreshTokenService.HashRefreshToken(refreshToken);


        var tokenEntity = new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            CreatedAt = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            RevokedAt = null,
            ReplacedByTokenId = null
        };
        await _refreshTokenRepository.AddAsync(tokenEntity,cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginUserResponse(
            accessToken,
            refreshToken);
    }

   
}
