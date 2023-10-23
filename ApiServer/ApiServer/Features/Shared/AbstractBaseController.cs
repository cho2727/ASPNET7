using ApiServer.Shared.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ApiServer.Features.Shared;

[ApiController]
[Route("[controller]")]
public abstract class AbstractBaseController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApiLogger _logger;

    public AbstractBaseController(IMediator mediator, IApiLogger logger)
    {
        _mediator = mediator;
        _logger = logger;
    }
}
