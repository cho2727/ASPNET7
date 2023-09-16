using ApiServer.Features.Shared;
using ApiServer.Shared.Interfaces;
using ApiServer.Shared.Models;
using Infrastructure;
using MediatR;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiServer.Features.User;

public class Refresh
{
    public class Command : IRequest<Response>
    {
        public string? AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; } = string.Empty;
    }

    public class Response : BaseResponse
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }

    public class CommandHandler : AbstractBaseHandler, IRequestHandler<Command, Response>
    {
        private readonly ITokenGenerator _tokenGenerator;

        public CommandHandler(ITokenGenerator tokenGenerator, ApiServerContext context, IConfiguration configuration, IApiLogger logger)
            : base(context, configuration, logger) => _tokenGenerator = tokenGenerator;

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var response = new Response { Result = false };
            var tokenPayload = _configuration.GetSection("TokenManagement").Get<TokenManagement>();
            if (tokenPayload == null) throw new NullReferenceException(nameof(tokenPayload));

            ClaimsPrincipal principal = ValidateToken(request.AccessToken,
                                                      tokenPayload.Audience,
                                                      tokenPayload.Issuer,
                                                      tokenPayload.Secret);
            var login = GetLoginInfo(request.AccessToken, request.RefreshToken, principal);
            // expiring
            login.UseFlag = false;
            var (AccessToken, RefreshToken) = ReCreateToken(login.User.Id);
            var refreshExpirationMinute = tokenPayload?.RefreshExpiration ?? 1440;
            AddLogin(login.User.Id, AccessToken, RefreshToken, refreshExpirationMinute);
            await _context.SaveChangesAsync(cancellationToken);

            response.Result = true;
            response.AccessToken = AccessToken;
            response.RefreshToken = RefreshToken;
            return response;
        }

        private Infrastructure.Models.Login GetLoginInfo
            (string accessToken, string refreshToken, ClaimsPrincipal principal)
        {
            var identity = (principal.Identity as ClaimsIdentity);
            var nameClaim = identity?.
                            Claims.
                            SingleOrDefault(c => c.Type == "Name");
            var login = GetLogin(l => l.AccessToken == accessToken
                                && l.RefreshToken == refreshToken
                                && l.RefreshTokenExpired > DateTime.UtcNow);
            if (login == null) throw new Exception("Invalid refreshToken");
            if (!login.User.Id.Equals(nameClaim?.Value, StringComparison.OrdinalIgnoreCase))
                throw new Exception("Invalid refreshToken");

            return login;
        }

        private static ClaimsPrincipal ValidateToken
            (string accessToken, string audience, string issuer, string secretKey)
        {
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidAudience = audience,
                ValidIssuer = issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                ValidateLifetime = false
            };
            return handler.ValidateToken(accessToken, validationParameters, out _);
        }

        private (string AccessToken, string RefreshToken) ReCreateToken(string id)
        {
            // expiring
            var newAccessToken = _tokenGenerator.Create(id);
            var newRefreshToken = _tokenGenerator.CreateRefreshToken();
            return (newAccessToken, newRefreshToken);
        }

    }
}
