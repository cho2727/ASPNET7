using ApiServer.Features.User;
using ApiServer.Helper;
using ApiServer.Shared.Extensions;
using Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using Xunit.Abstractions;

namespace UnitTest;

public class BaseTest
{
    protected readonly ITestOutputHelper _testOutput = default!;

    protected WebApplicationFactory<Program> _application = default!;
    protected HttpClient _client = default!;
    protected readonly ApiServerContext _context = default!;
    public BaseTest(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
        InitApplication();

        var scope = _application.Server.Services.GetService<IServiceScopeFactory>().CreateScope();
        _context = scope.ServiceProvider.GetRequiredService<ApiServerContext>();
    }
    private void InitApplication()
    {
        _application = new WebApplicationFactory<Program>();
        _client = _application.CreateClient();
    }
    ~BaseTest()
    {
        _context.Dispose();
        _client.Dispose();
        _application.Dispose();
    }

    protected async Task<Register.Response?> CommonRegister(string id = "Test", string password = "1234")
    {
        var request = new Register.Command
        {
            Id = id,
            Name = id,
            Password = password,
        };

        var data = new StringContent(content: request.SerializeToJson(), encoding: Encoding.UTF8, mediaType: "application/json");
        var responseMessage = await _client.PostAsync("/user/register", data);
        var response = await responseMessage.Content.ReadAsAsync<Register.Response>();
        return response;
    }

    protected async Task<Login.Response?> CommonLogin(string id = "Test", string password = "1234")
    {
        var request = new Login.Command
        {
            Id = id,
            Password = EncryptionHashHelper.EncryptPassword(password),
        };

        var data = new StringContent(content: request.SerializeToJson(), encoding: Encoding.UTF8, mediaType: "application/json");
        var responseMessage = await _client.PostAsync("/user/login", data);
        var response = await responseMessage.Content.ReadAsAsync<Login.Response>();
        return response;
    }
}
