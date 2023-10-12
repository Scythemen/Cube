using System;
using System.Buffers;
using System.Linq;
using System.Text;

namespace Cube.Utility;

public static class BufferExtensions
{
    public static SequencePosition? FirstOf(this ReadOnlySequence<byte> readOnlySequence, byte[] delimiter, bool advancePastDelimiter = false)
    {
        if (delimiter == null || delimiter.Length < 1 || readOnlySequence.Length < delimiter.Length)
        {
            return null;
        }

        var reader = new SequenceReader<byte>(readOnlySequence);
        var delimiterSpan = new ReadOnlySpan<byte>(delimiter);

        bool found = reader.TryReadTo(out ReadOnlySequence<byte> _, delimiterSpan, true);

        if (!found)
        {
            return null;
        }

        if (advancePastDelimiter)
        {
            return reader.Position;
        }
        else
        {
            return reader.Sequence.GetPosition(reader.Consumed - delimiter.Length);
        }
    }

    public static SequencePosition? FirstOf(this SequenceReader<byte> reader, byte[] delimiter, bool advancePastDelimiter = false)
    {
        var delimiterSpan = new ReadOnlySpan<byte>(delimiter);

        bool found = reader.TryReadTo(out ReadOnlySequence<byte> _, delimiterSpan, true);

        if (!found)
        {
            return null;
        }

        if (advancePastDelimiter)
        {
            return reader.Position;
        }
        else
        {
            return reader.Sequence.GetPosition(reader.Consumed - delimiter.Length);
        }
    }


    private static readonly string[] HexArray = Enumerable.Range(0, 256).Select(x => x.ToString("X02")).ToArray();

    public static string ToHex(this ReadOnlySequence<byte> bytes)
    {
        if (bytes.IsEmpty) return string.Empty;

        var sb = new StringBuilder((int)bytes.Length << 1);
        foreach (var memory in bytes)
        {
            foreach (var b in memory.Span)
            {
                sb.Append(HexArray[b]);
            }
        }

        return sb.ToString();
    }

    public static string ToHex(this MemorySequence<byte> memory)
    {
        if (memory.IsEmpty)
        {
            return string.Empty;
        }

        var sb = new StringBuilder(memory.Length << 1);
        foreach (var m in memory)
        {
            foreach (var b in m.Span)
            {
                sb.Append(HexArray[b]);
            }
        }

        return sb.ToString();
    }

    public static byte[] ToArray(this MemorySequence<byte> memory)
    {
        if (memory.IsEmpty)
        {
            return Array.Empty<byte>();
        }

        var bs = new byte[memory.Length];
        var i = 0;
        foreach (var m in memory)
        {
            foreach (var b in m.Span)
            {
                bs[i] = b;
                i++;
            }
        }

        return bs;
    }

}