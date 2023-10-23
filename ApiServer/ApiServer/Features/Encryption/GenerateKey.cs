using ApiServer.Features.Shared;
using ApiServer.Shared.Interfaces;
using Infrastructure;
using MediatR;
using System.Security.Cryptography;

namespace ApiServer.Features.Encryption;

public class GenerateKey
{
    public class Command : IRequest<Response>
    {
        public string Id { get; set; }
    }

    public class Response : BaseResponse
    {
        public string? PublicKey { get; set; }
    }

    public class CommandHandler : AbstractBaseHandler, IRequestHandler<Command, Response>
    {
        public CommandHandler(ApiServerContext context, IConfiguration configuration, IApiLogger logger) : base(context, configuration, logger)
        {
        }

        public async Task<Response> Handle(Command request, CancellationToken cancellationToken)
        {
            var response = new Response { Result = false };
            bool isLogin = false;
            Error? error;

            using var rsa = RSA.Create();
            var publicKey = rsa.ExportSubjectPublicKeyInfoPem();
            var privateKey = rsa.ExportPkcs8PrivateKeyPem();

            //_dictionary.AddOrUpdate(request.Id,
            //                        new RsaInfo(request.Id, publicKey, privateKey),
            //                        (key, oldValue) => new RsaInfo(request.Id, publicKey, privateKey));

            response.Result = true;
            response.PublicKey = publicKey;

            return response;
        }
    }
}
