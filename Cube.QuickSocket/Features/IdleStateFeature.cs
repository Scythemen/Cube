using Cube.Timer;

namespace Cube.QuickSocket;

public record class IdleStateFeature
{
    public int IdleMilliseconds { get; set; }
    public TimerTaskHandle TimerTaskHandler { get; set; }
}