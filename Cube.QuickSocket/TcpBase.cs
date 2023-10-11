using Cube.QuickSocket.AspNetCore.Transport;
using Cube.Timer;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Cube.QuickSocket;

public class TcpBase : IDisposable
{
    protected ILogger _logger;
    protected MiddlewareFeature _defaultMiddlewareFeature;
    protected readonly MiddlewareBuilder _middlewareBuilder;
    protected readonly CancellationTokenSource _stopTokenSource;
    protected readonly SocketTransportOptions _socketOptions;
    protected readonly IServiceProvider _serviceProvider;
    protected readonly ILoggerFactory _loggerFactory;

    internal static HashedWheelTimer Timer { get; } = new HashedWheelTimer(TimeSpan.FromMilliseconds(500), 512);

    protected TcpBase(SocketTransportOptions socketOptions = null, ILoggerFactory loggerFactory = null)
    {
        _socketOptions = socketOptions ?? new SocketTransportOptions();
        _loggerFactory = loggerFactory ?? LoggerFactory.Create(builder =>
        {
            builder.AddFilter("Microsoft", LogLevel.Warning)
                   .AddFilter("System", LogLevel.Warning)
                   .AddFilter("Cube.QuickSocket", LogLevel.Warning);
        });
        _logger = _loggerFactory.CreateLogger<TcpBase>();

        _middlewareBuilder = new MiddlewareBuilder(null, _loggerFactory.CreateLogger<MiddlewareBuilder>());
        _stopTokenSource = new CancellationTokenSource();
    }

    [ActivatorUtilitiesConstructor]
    protected TcpBase(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
        _socketOptions = serviceProvider.GetService<IOptions<SocketTransportOptions>>()?.Value ?? new SocketTransportOptions();
        _loggerFactory = _serviceProvider.GetService<ILoggerFactory>();
        _logger = _loggerFactory.CreateLogger<TcpBase>();

        _middlewareBuilder = new MiddlewareBuilder(serviceProvider);
        _stopTokenSource = new CancellationTokenSource();
    }


    protected async Task<ConnectionContext> ProcessConnection(ConnectionContext context)
    {
        _logger.LogTrace("New connection, Id:{}, R:{}, L:{}", context.ConnectionId, context.RemoteEndPoint, context.LocalEndPoint);

        context.Features.Set(_defaultMiddlewareFeature);

        // fire OnConnected event
        foreach (var m in _defaultMiddlewareFeature.Middlewares)
        {
            await m.OnConnected(context);
        }

        // middleware can be changed in IMiddleware 
        var finalFeatures = context.Features.Get<MiddlewareFeature>();
        var decoderContext = new DecoderContext(context);

        SequencePosition lastPosition = default;

        while (!_stopTokenSource.IsCancellationRequested)
        {
            try
            {
                var result = await context.Transport.Input.ReadAsync(_stopTokenSource.Token);
                if (result.IsCompleted || result.IsCanceled)
                {
                    break;
                }

                // reset idle time
                context.ResetIdleTimer();

                if (finalFeatures.DecoderDelegate == null)
                {
                    _logger.LogTrace("Id:{}, R:{}, L:{}, Decoder middleware delegate is null, consume the data: {} bytes",
                        context.ConnectionId, context.RemoteEndPoint, context.LocalEndPoint, result.Buffer.Length);

                    continue;
                }

                lastPosition = result.Buffer.End;

                long raw = result.Buffer.Length;

                // pass the context through the middlewares
                decoderContext.Input = result.Buffer;

                await finalFeatures.DecoderDelegate.Invoke(decoderContext);

                var consumed = raw - decoderContext.Input.Length;
                if (consumed > 0)
                {
                    context.Transport.Input.AdvanceTo(decoderContext.Input.Start);
                }
                else
                {
                    context.Transport.Input.AdvanceTo(result.Buffer.Start, result.Buffer.End);
                }
            }
            catch (Exception ex) when (ex is ConnectionAbortedException abort || ex is ConnectionResetException reset)
            {
                _logger.LogTrace("ConnectionId: {}, R:{}, L:{}, error: {}",
                    context.ConnectionId, context.RemoteEndPoint, context.LocalEndPoint, ex.Message);
                break;
            }
            catch (OperationCanceledException optCancel)
            {
                if (_stopTokenSource.IsCancellationRequested)
                {
                    _logger.LogTrace("ConnectionId: {}, R:{}, L:{}, error: {}",
                        context.ConnectionId, context.RemoteEndPoint, context.LocalEndPoint, optCancel.Message);
                    break;
                }
            }
            catch (Exception e)
            {
                // anyway consume the data 
                context.Transport.Input.AdvanceTo(lastPosition);

                _logger.LogTrace("ConnectionId: {}, R:{}, L:{}, error: {}",
                    context.ConnectionId, context.RemoteEndPoint, context.LocalEndPoint, e);
            }
        }

        // cancel idle time
        context.CancelIdleTimer();

        context.Abort();

        // fire OnClosed event from the last middleware
        for (int i = finalFeatures.Middlewares.Count - 1; i >= 0; i--)
        {
            finalFeatures.Middlewares[i].OnClosed(context);
        }

        return context;
    }


    public void Dispose()
    {
        Timer.Stop();
        Timer.Dispose();
    }
}