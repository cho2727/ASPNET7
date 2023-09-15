using ApiServer.Shared.Interfaces;
using System.Text;

namespace ApiServer.Shared.Middleware;

public class AutoLogger
{
    private readonly RequestDelegate _next;
    private readonly IApiLogger _logger;

    public AutoLogger(RequestDelegate next, IApiLogger logger)
    {
        _next = next;
        _logger = logger;
    }
    public async Task Invoke(HttpContext context)
    {
        var request = await FormatRequest(context.Request);
        _logger.LogDebug(request);

        var originalBodyStream = context.Response.Body;
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;
            await _next(context);
            var response = await FormatResponse(context.Response);
            _logger.LogDebug(response);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task<string> FormatRequest(HttpRequest request)
    {
        request.EnableBuffering();
        var body = request.Body;
        var buffer = new byte[Convert.ToInt32(request.ContentLength)];
        await request.Body.ReadAsync(buffer, 0, buffer.Length);
        var bodyAsText = Encoding.UTF8.GetString(buffer);
        body.Seek(0, SeekOrigin.Begin);
        request.Body = body;
        return $"{request.Scheme} endpoint = {request.Host}{request.Path} ," +
            $"query = {request.QueryString} ," +
            $"body = {bodyAsText}";
    }

    private async Task<string> FormatResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        string text = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);
        return $"{response.StatusCode}: {text}";
    }

}