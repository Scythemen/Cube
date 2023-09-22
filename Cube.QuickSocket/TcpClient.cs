using System.Net;
using Cube.QuickSocket.AspNetCore.Transport;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cube.QuickSocket;

public class TcpClient : TcpHelper
{
    private ConnectionContext? _context;

    public ConnectionContext ConnectionContext => _context;

    public TcpClient(
        IServiceProvider serviceProvider,
        IOptions<SocketTransportOptions> options = null,
        ILogger<TcpClient> logger = null) : base(serviceProvider, options)
    {
        _logger = logger;
        _logger ??= serviceProvider.GetService<ILoggerFactory>().CreateLogger<TcpClient>();
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
        ArgumentNullException.ThrowIfNull(instance);

        if (_context != null)
        {
            throw new NotSupportedException("The server has been started, failed to config middleware");
        }

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

        var clientFactory = new SocketConnectionFactory(_options, _serviceProvider.GetService<ILoggerFactory>());
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