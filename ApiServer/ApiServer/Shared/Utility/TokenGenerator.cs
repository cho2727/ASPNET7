using ApiServer.Shared.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Dynamic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiServer.Shared.Utility;

public class TokenGenerator : ITokenGenerator
{
    private readonly IConfiguration _configuration;

    public TokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string Create(string id, string role = "user")
    {
        dynamic token = new ExpandoObject();
        var tokenSection = _configuration.GetSection("TokenManagement");

        token.Secret = tokenSection.GetValue<string>("Secret");
        token.Issuer = tokenSection.GetValue<string>("Issuer");
        token.Audience = tokenSection.GetValue<string>("Audience");
        token.AccessExpiration = tokenSection.GetValue<int>("AccessExpiration");
        token.RefreshExpiration = tokenSection.GetValue<int>("RefreshExpiration");

        if (token is null) throw new Exception("TokenManagement configuration need. check your configuration");
        var now = DateTime.UtcNow;

        var claims = new[]
        {
            new Claim("Name", id),
            new Claim("Role", role),
            new Claim("Ticks", now.AddMinutes(token.AccessExpiration).Ticks.ToString())
        };

        var jwtToken = new JwtSecurityToken(
        token.Issuer,
        token.Audience,
        claims,
        expires: now.AddMinutes(token.AccessExpiration),
        signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(token.Secret)), SecurityAlgorithms.HmacSha512Signature));
        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }

    public string CreateRefreshToken() => Guid.NewGuid().ToString("N");
}
