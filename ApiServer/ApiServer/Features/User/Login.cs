using ApiServer.Shared.Interfaces;
using ApiServer.Shared.Models;
using Infrastructure;
using Infrastructure.Migrations;
using MediatR;

namespace ApiServer.Features.User;

public class Login
{
    public class Command : IRequest<Response>
    {
        public string Id { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class Response
    {
        public bool Result { get; set; }
        public Error? Error { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
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
            var response = new Response { Result = false };
            string? accessToken = null;
            string? refreshToken = null;

            bool isLogin = false;
            Error? error;

            var findUser = Exist(request.Id);
            if(findUser != null)
            {
                isLogin = findUser.Password == request.Password ? true : false;
                if(isLogin)
                {
                    accessToken = _tokenGenerator.Create(request.Id);
                    refreshToken = _tokenGenerator.CreateRefreshToken();
                    AddLogin(request, accessToken, refreshToken);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    error = new Error()
                    {
                        Code = "02",
                        Message = $"Login Failed, check your passwd"
                    };
                }
            }
            else
            {
                error = new Error()
                {
                    Code = "01",
                    Message = $"Con not found LoinId = {request.Id}"
                };
            }

            response.Result = isLogin;
            response.AccessToken = accessToken;
            response.RefreshToken = refreshToken;
            return response;
        }

        private Infrastructure.Models.User? Exist(string id)
        {
            return _context.Users?
                .Where(m => m.Id == id && m.UseFlag == true)
                .SingleOrDefault();
        }

        private void AddLogin(Command item, string? accessToken, string? refreshToken)
        {
            var expirationMinute = _configuration.GetSection("TokenManagement")
                .Get<TokenManagement>()?.RefreshExpiration ?? 1440;

            var refreshTokenExpired = DateTime.UtcNow.AddMinutes(expirationMinute);

            var findUser = _context.Users.Where(u => u.Id == item.Id).SingleOrDefault();
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
