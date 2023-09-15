using ApiServer.Helper;
using ApiServer.Shared.Interfaces;
using Infrastructure;
using MediatR;
using Microsoft.Identity.Client;

namespace ApiServer.Features.User;

public class Logout
{
    public class Command : IRequest<Response>
    {
        public string AccessToken { get; set; } = string.Empty;
    }

    public class Response
    {
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
            var response = new Response { Result = false };
            var findLogin = Exist(request.AccessToken);
            if(findLogin != null)
            {
                findLogin.UseFlag = false;
                _context.Logins.Update(findLogin);
                await _context.SaveChangesAsync();
            }           

            response.Result = true;

            return response;
        }

        private Infrastructure.Models.Login? Exist(string accessToken)
        {
            return _context.Logins?
                .Where(l => l.AccessToken == accessToken)
                .SingleOrDefault();
        }


    }
}
