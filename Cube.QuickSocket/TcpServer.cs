using Cube.QuickSocket.AspNetCore.Transport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Cube.QuickSocket;

public class TcpServer : TcpBase
{
    private SocketConnectionListener _listener = null;

    public TcpServer(SocketTransportOptions options = null, ILoggerFactory loggerFactory = null)
        : base(options, loggerFactory)
    {
        _logger = _loggerFactory.CreateLogger<TcpServer>();
    }


    [ActivatorUtilitiesConstructor]
    public TcpServer(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _logger = _loggerFactory.CreateLogger<TcpServer>();
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
        if (_listener != null)
        {
            throw new NotSupportedException("The server has been started, failed to config middleware");
        }

        ArgumentNullException.ThrowIfNull(instance);

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
            _listener = new SocketConnectionListener(ipEndPoint, _socketOptions, _loggerFactory);
            _listener.Bind();

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
    }


    public new async void Dispose()
    {
        await StopAsync();

        foreach (var m in _defaultMiddlewareFeature.Middlewares)
        {
            m.Dispose();
        }

        base.Dispose();
    }
}