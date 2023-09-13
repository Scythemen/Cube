using System.Buffers;
using Microsoft.AspNetCore.Connections;

namespace Cube.QuickSocket;

public record class DecoderContext
{
    public ConnectionContext Context { get; private set; }
    public ReadOnlySequence<byte> Input { get; set; } = ReadOnlySequence<byte>.Empty;


// todo pooledObject?
    public DecoderContext(ConnectionContext context)
    {
        Context = context;
    }
}