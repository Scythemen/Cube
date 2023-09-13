namespace Cube.QuickSocket;

public struct SendResult
{
    public bool Completed { get; set; }
    public Exception Exception { get; set; }
    public string Message { get; set; }
}