using ApiServer.Features.User;
using ApiServer.Helper;
using ApiServer.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Xunit.Abstractions;

namespace UnitTest;

public class UserTest : BaseTest
{
    public UserTest(ITestOutputHelper testOutput) : base(testOutput)
    {
    }

    [Fact]
    public async Task Register()
    {
        await _context.Users.ExecuteDeleteAsync();
        var request = new Register.Command
        {
            Id = "Test",
            Name = "Test",
            Password = "1234"
        };

        var data = new StringContent(content: request.SerializeToJson(),
                                    encoding: Encoding.UTF8,
                                    mediaType: "application/json");
        var responseMessage = await _client.PostAsync("user/register", data);
        _testOutput.WriteLine("결과CONTENT:");
        _testOutput.WriteLine(responseMessage.ToString());
        Assert.True(responseMessage.IsSuccessStatusCode);
        var response = await responseMessage.Content.ReadAsAsync<Register.Response>();
        _testOutput.WriteLine("결과데이터:");
        _testOutput.WriteLine(response?.SerializeToJson());
        Assert.True(response?.Result);

        await _context.Users.ExecuteDeleteAsync();
    }


    [Fact]
    public async Task Login()
    {
        await _context.Logins.ExecuteDeleteAsync();
        await _context.Users.ExecuteDeleteAsync();

        await CommonRegister();
        var request = new Login.Command
        {
            Id = "Test",
            Password = EncryptionHashHelper.EncryptPassword("1234"),
        };

        var data = new StringContent(content: request.SerializeToJson(), 
                                    encoding: Encoding.UTF8, 
                                    mediaType: "application/json");

        var responseMessage = await _client.PostAsync("/user/login", data);
        _testOutput.WriteLine(responseMessage.ToString());
        Assert.True(responseMessage.IsSuccessStatusCode);
        var response = await responseMessage.Content.ReadAsAsync<Login.Response>();
        _testOutput.WriteLine(response?.SerializeToJson());
        Assert.True(response?.Result);

        await _context.Logins.ExecuteDeleteAsync();
        await _context.Users.ExecuteDeleteAsync();
    }


    [Fact]
    public async Task Logout()
    {
        await _context.Logins.ExecuteDeleteAsync();
        await _context.Users.ExecuteDeleteAsync();

        await CommonRegister();
        var preResponse = await CommonLogin();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", preResponse?.AccessToken);
        var responseMessage = await _client.PostAsync("/user/logout", null);
        _testOutput.WriteLine(responseMessage.ToString());
        Assert.True(responseMessage.IsSuccessStatusCode);
        var response = await responseMessage.Content.ReadAsAsync<Logout.Response>();
        _testOutput.WriteLine(response?.SerializeToJson());
        Assert.True(response?.Result);

        _client.DefaultRequestHeaders.Remove("Bearer");
        await _context.Logins.ExecuteDeleteAsync();
        await _context.Users.ExecuteDeleteAsync();
    }

    [Fact]
    public async Task Refresh()
    {
        await _context.Logins.ExecuteDeleteAsync();
        await _context.Users.ExecuteDeleteAsync();

        await CommonRegister();
        var preResponse = await CommonLogin();

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", preResponse?.AccessToken);
        var responseMessage = await _client.PostAsync($"/user/refresh?refreshToken={preResponse?.RefreshToken}", null);
        _testOutput.WriteLine(responseMessage.ToString());
        Assert.True(responseMessage.IsSuccessStatusCode);
        var response = await responseMessage.Content.ReadAsAsync<Refresh.Response>();
        _testOutput.WriteLine(response?.SerializeToJson());
        Assert.True(response?.Result);

        _client.DefaultRequestHeaders.Remove("Bearer");
        await _context.Logins.ExecuteDeleteAsync();
        await _context.Users.ExecuteDeleteAsync();
    }
}
