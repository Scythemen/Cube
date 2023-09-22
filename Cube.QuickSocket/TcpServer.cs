using System.Net;
using Cube.QuickSocket.AspNetCore.Transport;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;

namespace Cube.QuickSocket;

public class TcpServer : TcpHelper
{
    private IConnectionListener _listener = null;

    public TcpServer(
        IServiceProvider serviceProvider,
        IOptions<SocketTransportOptions> options = null,
        ILogger<TcpServer> logger = null) : base(serviceProvider, options)
    {
        _logger = logger;
        _logger ??= serviceProvider.GetService<ILoggerFactory>().CreateLogger<TcpServer>();
    }


    public TcpServer UseMiddleware<T>() where T : IMiddleware
    {
        if (_listener != null)
        {
            throw new NotSupportedException("The server has been started, failed to config middleware");
        }

        _middlewareBuilder.Use<T>();
        return this;
    }


    public TcpServer UseMiddleware(IMiddleware instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (_listener != null)
        {
            throw new NotSupportedException("The server has been started, failed to config middleware");
        }

        _middlewareBuilder.Use(instance);
        return this;
    }


    public async ValueTask<TcpServer> StartAsync(IPEndPoint ipEndPoint)
    {
        if (_listener != null)
        {
            _logger.LogInformation("Tcp server is listening.");
            return this;
        }

        _logger.LogInformation("Starting tcp server...");

        try
        {
            var server = new SocketTransportFactory(_options, _serviceProvider.GetService<ILoggerFactory>());
            _listener = await server.BindAsync(ipEndPoint, _stopTokenSource.Token);
            _logger.LogInformation("Bind to {}...ok", ipEndPoint);
        }
        catch (Exception e)
        {
            _logger.LogError("Binding to {}, error: {}", ipEndPoint, e.Message);
            throw;
        }

        _defaultMiddlewareFeature = _middlewareBuilder.BuildAsMiddlewareFeature();

        if (_defaultMiddlewareFeature.Middlewares.Count == 0)
        {
            _logger.LogWarning("Tcp server {} middleware not set.", ipEndPoint);
        }

        _ = AcceptConnection();

        return this;
    }


    private async Task AcceptConnection()
    {
        while (!_stopTokenSource.IsCancellationRequested)
        {
            var context = await _listener!.AcceptAsync(_stopTokenSource.Token);
            if (context != null)
            {
                _ = ProcessConnection(context);
            }
        }

        _stopTokenSource.Cancel();
    }

    public async Task StopAsync()
    {
        _stopTokenSource?.Cancel();
        _logger.LogInformation("Shutdown tcp server {}...", _listener?.EndPoint);
        if (_listener != null)
        {
            await _listener.UnbindAsync();
            await _listener.DisposeAsync();
            _listener = null;
        }

        foreach (var m in _defaultMiddlewareFeature.Middlewares)
        {
            m.Dispose();
        }
    }


    public new void Dispose()
    {
        StopAsync();
        base.Dispose();
    }
}