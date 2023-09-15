using ApiServer.Shared.Injectables;

namespace ApiServer.Shared.Interfaces;

public interface ITokenGenerator : ISingletonService
{
    string Create(string id, string role="user");

    string CreateRefreshToken();
}
