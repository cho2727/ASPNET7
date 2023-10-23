using ApiServer.Features.Shared;
using ApiServer.Shared.Interfaces;
using MediatR;

namespace ApiServer.Features.Encryption
{
    public class EncryptController : AbstractBaseController
    {
        public EncryptController(IMediator mediator, IApiLogger logger)
            : base(mediator, logger)
        {
        }

    }
}
