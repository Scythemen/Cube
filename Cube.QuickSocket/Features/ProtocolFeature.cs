namespace Cube.QuickSocket;

public record ProtocolFeature
{
    /// <summary>
    /// Represents the protocol of the connection
    /// </summary>
    public string Protocol { get; set; } = string.Empty;

    /// <summary>
    /// A temporary counter for matching/identifying the protocol.
    /// Provide a gentle way to close the invalid connection.
    /// Start from zero, if the counter is greater then 3, then it's a invalid connection, should be closed.
    /// </summary>
    public int MatchTimes { get; set; } = 0;
}