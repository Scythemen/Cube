using Cube.Utility;
using Microsoft.AspNetCore.Connections;

namespace Cube.QuickSocket;

public record class EncoderContext
{
    public ConnectionContext Context { get; private set; }
    public MemorySequence<byte> Output { get; private set; }
// todo pooledObject?

    public EncoderContext(ConnectionContext context)
    {
        Output = MemorySequence<byte>.Empty;
        Context = context;
    }

    public EncoderContext SetOutput(MemorySequence<byte> output)
    {
        Output = output;
        return this;
    }
}