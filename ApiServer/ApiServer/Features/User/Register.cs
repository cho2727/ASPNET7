using ApiServer.Features.Shared;
using ApiServer.Helper;
using ApiServer.Shared.Interfaces;
using Infrastructure;
using MediatR;

namespace ApiServer.Features.User;

public class Register
{
    public class Command : IRequest<Response>
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
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

            _context.Users.Add(new()
            {
                Id = request.Id,
                Name = request.Name,
                Password = EncryptionHashHelper.EncryptPassword(request.Password),
                UseFlag = true
            });

            await _context.SaveChangesAsync();

            response.Result = true;

            return response;
        }


    }
}