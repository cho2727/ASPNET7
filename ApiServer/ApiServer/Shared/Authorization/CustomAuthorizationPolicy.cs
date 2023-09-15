using ApiServer.Shared.Interfaces;
using ApiServer.Shared.Models;
using Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace ApiServer.Shared.Authorization;

public class CustomAuthorizationPolicyRequirement : IAuthorizationRequirement
{
}

public class CustomAuthorizationPolicyHandler : AuthorizationHandler<CustomAuthorizationPolicyRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApiServerContext _context;
    private readonly IApiLogger _logger;
    private readonly IConfiguration _configuration;

    public CustomAuthorizationPolicyHandler(IHttpContextAccessor httpContextAccessor,
                                            ApiServerContext context,
                                            IApiLogger logger,
                                            IConfiguration configuration )
    {
        _httpContextAccessor = httpContextAccessor;
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CustomAuthorizationPolicyRequirement requirement)
    {
        var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].ToString();

        AuthenticationHeaderValue.TryParse(token, out var tokenValue);
        if(await ValidateTokenAsync(tokenValue?.Parameter))
        {
            _logger.LogDebug("authorization successed");
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogDebug("authorization failed");
            context.Fail();
        }
    }

    private async ValueTask<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenPayload = _configuration.GetSection("TokenManagement").Get<TokenManagement>();
            var handler = new JwtSecurityTokenHandler();
             TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidAudience = tokenPayload?.Audience,
                ValidIssuer = tokenPayload?.Issuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(tokenPayload.Secret))
            };

            var principal = handler.ValidateToken(token, validationParameters, out var jwtToken);

            var identity = (principal.Identity as ClaimsIdentity);
            var nameClaim = identity?.
                            Claims.SingleOrDefault(c => c.Type == "Name");

            return await IsValidationTokenAsync(nameClaim?.Value ?? string.Empty, token);
        }
        catch (Exception ex)
        {
            _logger.LogInformation(ex.Message);
            return false;
        }
    }

    private async Task<bool> IsValidationTokenAsync(string id, string token)
    {
        var users = await _context.Users.Where(u => u.Id == id && u.UseFlag == true)
                                        .Include(u => u.Logins.Where(l => l.AccessToken == token && l.UseFlag == true))
                                        .ToListAsync();

        if (users.Count > 0)
            return users.Single().Logins?.Count > 0;

        return false;
    }
}
