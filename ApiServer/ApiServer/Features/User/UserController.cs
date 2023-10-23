using ApiServer.Helper;
using Azure.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace ApiServer.Features.User;

[ApiController]
[Route("[controller]")]
[Authorize("CustomAuthorizationPolicy")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;

    public UserController(IMediator mediator)
    {
        this._mediator = mediator;
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    [ProducesResponseType(typeof(Login.Response), StatusCodes.Status200OK)]
    public async Task<IActionResult> Login(Login.Command request)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var response = await _mediator.Send(request);
        return response.Result ? Ok(response) : Unauthorized();
    }

    [HttpGet("Hello")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AuthHello() => await Task.FromResult(Ok("Hello"));

    [HttpGet("authentication")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Authentication() => await Task.FromResult(Ok("Hello"));

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(Register.Response), StatusCodes.Status200OK)]
    public async Task<IActionResult> Register(Register.Command request)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var response = await _mediator.Send(request);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("password-generate")]
    public async Task<IActionResult> PasswordGenerate(string pasword)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        return await Task.FromResult(Ok(EncryptionHashHelper.EncryptPassword(pasword)));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var token = Request?.Headers["Authorization"];
        AuthenticationHeaderValue.TryParse(token, out var tokenValue);

        var response = await _mediator.Send(new Logout.Command() { AccessToken = tokenValue?.Parameter ?? string.Empty});
        return response.Result ? Ok(response) : Unauthorized();
    }

    [AllowAnonymous]
    [ProducesResponseType(typeof(Refresh.Response), StatusCodes.Status200OK)]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(string refreshToken)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var token = Request?.Headers["Authorization"];
        AuthenticationHeaderValue.TryParse(token, out var tokenValue);

        var response = await _mediator.Send(new Refresh.Command { AccessToken = tokenValue?.Parameter ?? string.Empty, RefreshToken = refreshToken });

        return Ok(response);
    }
}


