using Cube.Utility;

namespace Cube.SimpleProtocol
{
    public class SimpleFrame : ICloneable
    {
        public SimpleFrame(byte[]? head, byte[]? payload, byte[]? tail)
        {
            Payload = payload;
            Head = head;
            Tail = tail;
        }

        public byte[]? Head { get; private set; } = Array.Empty<byte>();
        public byte[]? Payload { get; private set; } = Array.Empty<byte>();
        public byte[]? Tail { get; private set; } = Array.Empty<byte>();

        public object Clone()
        {
            return new SimpleFrame(Head, Payload, Tail);
        }

        public override string ToString()
        {
            return string.Format("[{6}: {0}={1}, {2}={3}, {4}={5}]",
                nameof(Head), Head.ToHex(),
                nameof(Payload), Payload.ToHex(),
                nameof(Tail), Tail.ToHex(),
                nameof(SimpleFrame));
        }
    }
}