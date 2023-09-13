using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Cube.QuickSocket;

public record class FlowAnalyzeMiddlewareOptions
{
    private int _interval = 5;

    /// <summary>
    ///  Seconds in [1,30]
    /// </summary>
    public int Interval
    {
        get => _interval;
        set
        {
            _interval = value < 1 ? 1 : value;
            _interval = _interval > 30 ? 30 : _interval;
        }
    }


    public bool Logging { get; set; } = true;
    
    public LogLevel LogLevel { get; set; } = LogLevel.Trace;
}