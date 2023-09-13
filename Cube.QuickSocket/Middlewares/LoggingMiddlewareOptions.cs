using Microsoft.Extensions.Logging;

namespace Cube.QuickSocket;

public record class LoggingMiddlewareOptions
{
    public bool Logging { get; set; } = true;

    public LogLevel LogLevel { get; set; } = LogLevel.Trace;
}