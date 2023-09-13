namespace Cube.QuickSocket;

public sealed class FlowAnalyzeFeature
{
    public long TotalInputBytes { get; set; } = 0;
    public long TotalOutputBytes { get; set; } = 0;
    public long Connections { get; set; } = 0;
    public double InputRate { get; set; } = 0.0D;
    public double OutputRate { get; set; } = 0.0D;


    public override string ToString()
    {
        return string.Format("{0}:{1}, {2}:{3}, {6}:{7}, {4}:{5:F2} B/s, {8}:{9:F2} B/s",
            nameof(Connections), Connections,
            nameof(TotalInputBytes), TotalInputBytes,
            nameof(InputRate), InputRate,
            nameof(TotalOutputBytes), TotalOutputBytes,
            nameof(OutputRate), OutputRate
        );
    }
}