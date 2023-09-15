using ApiServer.Helper;
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
        private readonly ApiServerContext _context;

        public CommandHandler(ApiServerContext context)
        {
            _context = context;
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