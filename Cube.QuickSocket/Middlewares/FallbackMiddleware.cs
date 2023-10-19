using Cube.Utility;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cube.QuickSocket;

public sealed class FallbackMiddleware : IMiddleware
{
    private readonly ILogger _logger;
    private readonly FallbackMiddlewareOptions _option;

    public FallbackMiddleware(
        ILogger<FallbackMiddleware> logger = null,
        IOptions<FallbackMiddlewareOptions> options = null)
    {
        _option = options == null ? new FallbackMiddlewareOptions() : options.Value;
        _logger = logger == null ? NullLogger.Instance : logger;
    }

    public Task OnConnected(ConnectionContext connection)
    {
        return Task.CompletedTask;
    }


    public Task DecodeAsync(DecoderMiddlewareDelegate next, DecoderContext context)
    {
        if (_option.Logging && _logger.IsEnabled(_option.LogLevel))
        {
            _logger.Log(_option.LogLevel, "ConnectionId:{}, R:{}, L:{}, Echo, hex({}): {} ",
                context.Context.ConnectionId, context.Context.RemoteEndPoint, context.Context.LocalEndPoint,
                context.Input.Length, context.Input.ToHex());
        }

        if (_option.Echo)
        {
            _ = context.Context.Send(context.Input, SendOptions.SpecificMiddleware, typeof(FallbackMiddleware), _logger);
        }

        // set the protocol to keep the connection alive
        var protocolFeature = context.Context.Features.Get<ProtocolFeature>();
        if (protocolFeature == null)
        {
            protocolFeature = new ProtocolFeature();
            context.Context.Features.Set(protocolFeature);
        }
        else
        {
            if (protocolFeature.MatchTimes >= 2)
            {
                protocolFeature.Protocol = nameof(FallbackMiddleware);
                if (_option.Logging && _logger.IsEnabled(_option.LogLevel))
                {
                    _logger.Log(_option.LogLevel, "Set the protocol to connection to keep the connection alive, connectionId:{}, R:{}, L:{}",
                        context.Context.ConnectionId, context.Context.RemoteEndPoint, context.Context.LocalEndPoint);
                }
            }
        }

        context.Input = context.Input.Slice(context.Input.End);

        return Task.CompletedTask;
    }

    public Task EncodeAsync(EncoderMiddlewareDelegate next, EncoderContext context)
    {
        return next(context);
    }

    public Task OnClosed(ConnectionContext connection)
    {
        return Task.CompletedTask;
    }

    public Task OnIdle(ConnectionContext connection)
    {
        return Task.CompletedTask;
    }


    public void Dispose()
    {
    }
}