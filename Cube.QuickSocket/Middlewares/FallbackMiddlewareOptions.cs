using Microsoft.Extensions.Logging;

namespace Cube.QuickSocket;

public record class FallbackMiddlewareOptions
{
    public bool Echo { get; set; } = true;

    public bool Logging { get; set; } = false;

    public LogLevel LogLevel { get; set; } = LogLevel.Debug;
}