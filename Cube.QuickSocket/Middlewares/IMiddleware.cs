using Microsoft.AspNetCore.Connections;

namespace Cube.QuickSocket;

public delegate Task DecoderMiddlewareDelegate(DecoderContext context);

public delegate Task EncoderMiddlewareDelegate(EncoderContext context);

public interface IMiddleware : IDisposable
{
    Task OnConnected(ConnectionContext connection);
    Task DecodeAsync(DecoderMiddlewareDelegate next, DecoderContext context);
    Task EncodeAsync(EncoderMiddlewareDelegate next, EncoderContext context);
    Task OnClosed(ConnectionContext connection);
    Task OnIdle(ConnectionContext connection);
}