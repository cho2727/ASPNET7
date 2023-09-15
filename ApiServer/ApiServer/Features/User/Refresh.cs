using ApiServer.Helper;
using ApiServer.Shared.Interfaces;
using ApiServer.Shared.Models;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
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

    public class Response
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public bool Result { get; set; }
        public Error? Error { get; set; }
    }

    public class Error
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class CommandHandler : IRequestHandler<Command, Response>
    {
        private readonly ITokenGenerator _tokenGenerator;
        private readonly ApiServerContext _context;
        private readonly IConfiguration _configuration;

        public CommandHandler(ITokenGenerator tokenGenerator, ApiServerContext context, IConfiguration configuration)
        {
            _tokenGenerator = tokenGenerator;
            _context = context;
            _configuration = configuration;
        }
        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var tokenPayload = _configuration.GetSection("TokenManagement").Get<TokenManagement>();
            var handler = new JwtSecurityTokenHandler();
            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidAudience = tokenPayload.Audience,
                ValidIssuer = tokenPayload.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenPayload.Secret)),
                ValidateLifetime = false
            };

            var principal = handler.ValidateToken(request.AccessToken, validationParameters, out var jwtToken);
            var identity = (principal.Identity as ClaimsIdentity);
            var nameClaim = identity?.
                            Claims.
                            SingleOrDefault(c => c.Type == "Name");

            Infrastructure.Models.Login? findLogin = FindLoginAndCheckValidToken(request, nameClaim);
            Expiring(findLogin);
            (var accessToken, var refreshToken) = Recreate(nameClaim?.Value);

            Add(nameClaim?.Value, accessToken, refreshToken);
            await _context.SaveChangesAsync();


            var response = new Response { Result = false };
            response.Result = true;
            response.AccessToken = accessToken;
            response.RefreshToken = refreshToken;

            return response;
        }

        private Infrastructure.Models.Login? FindLoginAndCheckValidToken(Command request, Claim? nameClaim)
        {
            var findLogin = Find(request.AccessToken, request.RefreshToken);
            if (findLogin == null) throw new Exception("Invalid refreshToken");
            if (!findLogin.User.Id.Equals(nameClaim?.Value, StringComparison.OrdinalIgnoreCase)) 
                throw new Exception("Invalid refreshToken");
            return findLogin;
        }

        private (string AccessToken, string RefreshToken) Recreate(string id) => (_tokenGenerator.Create(id), _tokenGenerator.CreateRefreshToken());

        private void Expiring(Infrastructure.Models.Login login) => login.UseFlag = false;

        private Infrastructure.Models.Login? Find(string accessToken, string refreshToken)
        {
            return _context.Logins?
            .Where(l => l.AccessToken == accessToken
                    && l.RefreshToken == refreshToken
                    && l.RefreshTokenExpired > DateTime.UtcNow)
            .Include(l => l.User)
            .SingleOrDefault();
        }

        private void Add(string id, string? accessToken, string? refreshToken)
        {
            var expirationMinute = _configuration.GetSection("TokenManagement").Get<TokenManagement>()?.RefreshExpiration ?? 1440;

            var refreshTokenExpired = DateTime.UtcNow.AddMinutes(expirationMinute);

            var findUser = _context.Users.Where(u => u.Id == id).SingleOrDefault();
            var login = new Infrastructure.Models.Login
            {
                User = findUser,
                UseFlag = true,
                AccessToken = accessToken ?? string.Empty,
                RefreshToken = refreshToken ?? string.Empty,
                RefreshTokenExpired = refreshTokenExpired
            };
            _context.Logins.Add(login);
        }
    }
}
