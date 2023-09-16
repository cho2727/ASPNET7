using ApiServer.Features.Shared;
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

    public class Response : BaseResponse
    {
    }

    public class CommandHandler : AbstractBaseHandler, IRequestHandler<Command, Response>
    {
        public CommandHandler(ApiServerContext context, IConfiguration configuration, IApiLogger logger)
            : base(context, configuration, logger)
        {
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
