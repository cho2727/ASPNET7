using ApiServer.Shared.Interfaces;
using MediatR;
using System.Diagnostics;
using System.Text.Json;

namespace ApiServer.Shared.Behaviors;

public class MediatRAutoLogger<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : MediatR.IRequest<TResponse>
    //where TResponse : new()
{
    private readonly IApiLogger _logger;
    private readonly IConfiguration _configuration;

    public MediatRAutoLogger(IApiLogger logger, IConfiguration configuration)
    {
        _logger = logger;
        this._configuration = configuration;
    }

    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var islog = _configuration.GetSection("Serilog")?.GetValue<bool>("AutoLog") ?? false;
        if(islog)
        {
            string requestName = typeof(TRequest).Name;
            string uniqueId = Guid.NewGuid().ToString();
            _logger.LogDebug($"Begin Request Id:{uniqueId}, request name:{requestName},\nRequest={JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true })}");
            var timer = new Stopwatch();
            timer.Start();
            var response = next();
            timer.Stop();

            _logger.LogDebug($"End Request Id:{uniqueId}, request name:{requestName},\nResponse={JsonSerializer.Serialize(response.Result, new JsonSerializerOptions { WriteIndented = true })}" +
                $"\ntotal elapsed time: {timer.ElapsedMilliseconds}");
            return response;
        }
        else
        {
            var response = next();
            return response;
        }

        // return Task.FromResult(new TResponse());
    }
}
