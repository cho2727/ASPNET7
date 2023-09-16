using ApiServer.Shared.Interfaces;
using ApiServer.Shared.Models;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ApiServer.Features.Shared;

public abstract class AbstractBaseHandler
{
    protected readonly ApiServerContext _context;
    protected readonly IConfiguration _configuration;
    protected readonly IApiLogger _logger;

    public AbstractBaseHandler(ApiServerContext context, IConfiguration configuration, IApiLogger logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    //private Infrastructure.Models.Login? Find(string accessToken, string refreshToken)
    //{
    //    return GetLogin(l => l.AccessToken == accessToken
    //            && l.RefreshToken == refreshToken
    //            && l.RefreshTokenExpired > DateTime.UtcNow);
    //    //return _context.Logins?
    //    //.Where(l => l.AccessToken == accessToken
    //    //        && l.RefreshToken == refreshToken
    //    //        && l.RefreshTokenExpired > DateTime.UtcNow)
    //    //.Include(l => l.User)
    //    //.SingleOrDefault();
    //}

    protected Infrastructure.Models.Login? GetLogin(Expression<Func<Infrastructure.Models.Login, bool>> predicate)
    => _context.Logins?.Where(predicate)
        .Include(l => l.User)
        .SingleOrDefault();


    protected void AddLogin(string id, string? accessToken, string? refreshToken, int refreshExpirationMinute)
    {
        var refreshTokenExpired = DateTime.UtcNow.AddMinutes(refreshExpirationMinute);

        var findUser = _context.Users.Where(u => u.Id == id).SingleOrDefault();
        if (findUser == null) throw new Exception("Can not find user");
        var login = new Infrastructure.Models.Login
        {
            User = findUser,
            UseFlag = true,
            AccessToken = accessToken ?? string.Empty,
            RefreshToken = refreshToken ?? string.Empty,
            RefreshTokenExpired = refreshTokenExpired
        };
        _context.Logins.Add(login);
    }
}
