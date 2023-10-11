using System.Net;
using Cube.QuickSocket.AspNetCore.Transport;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cube.QuickSocket;

public class TcpClient : TcpBase
{
    private ConnectionContext? _context;
    public ConnectionContext ConnectionContext => _context;


    public TcpClient(SocketTransportOptions options = null, ILoggerFactory loggerFactory = null)
        : base(options, loggerFactory)
    {
        _logger = _loggerFactory?.CreateLogger<TcpClient>();
    }

    [ActivatorUtilitiesConstructor]
    public TcpClient(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        _logger = _loggerFactory?.CreateLogger<TcpClient>();
    }


    public TcpClient UseMiddleware<T>() where T : IMiddleware
    {
        if (_context != null)
        {
            throw new NotSupportedException("The server has been started, failed to config middleware");
        }

        _middlewareBuilder.Use<T>();
        return this;
    }


    public TcpClient UseMiddleware(IMiddleware instance)
    {
        if (_context != null)
        {
            throw new NotSupportedException("The server has been started, failed to config middleware");
        }

        ArgumentNullException.ThrowIfNull(instance);

        _middlewareBuilder.Use(instance);
        return this;
    }


    public async ValueTask<TcpClient> ConnectAsync(IPEndPoint ipEndPoint)
    {
        if (_context != null)
        {
            _logger.LogInformation("The connection is connected to {}", _context.RemoteEndPoint);
            return this;
        }

        var clientFactory = new SocketConnectionFactory(Options.Create(_socketOptions), _loggerFactory);
        try
        {
            _context = await clientFactory.ConnectAsync(ipEndPoint, _stopTokenSource.Token);
            _logger.LogDebug("connect to {}... ok", ipEndPoint);
        }
        catch (Exception e)
        {
            _logger.LogError("Connecting to {}, error: {}", ipEndPoint, e);
            throw;
        }

        _defaultMiddlewareFeature = _middlewareBuilder.BuildAsMiddlewareFeature();

        if (_defaultMiddlewareFeature.Middlewares.Count == 0)
        {
            _logger.LogWarning("[{}]: Middleware not set.", ipEndPoint);
        }

        _ = ProcessConnection(_context);

        return this;
    }


    public async Task StopAsync()
    {
        _stopTokenSource?.Cancel();
        if (_context != null)
        {
            await _context.DisposeAsync();
            _context = null;
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