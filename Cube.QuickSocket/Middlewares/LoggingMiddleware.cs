using System.Buffers;
using System.Globalization;
using System.Text;
using Cube.Utility;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cube.QuickSocket;

public sealed class LoggingMiddleware : IMiddleware
{
    private readonly ILogger _logger;
    private LoggingMiddlewareOptions _options;

    public LoggingMiddleware(IOptions<LoggingMiddlewareOptions> options = null, ILogger<LoggingMiddleware> logger = null)
    {
        _logger = logger;
        _logger ??= NullLogger.Instance;
        _options = options == null ? new LoggingMiddlewareOptions() : options.Value;
    }

    public Task OnConnected(ConnectionContext connection)
    {
        return Task.CompletedTask;
    }

    public Task DecodeAsync(DecoderMiddlewareDelegate next, DecoderContext context)
    {
        Log("Input", context.Input);
        return next(context);
    }

    public Task EncodeAsync(EncoderMiddlewareDelegate next, EncoderContext context)
    {
        Log("Output", context.Output);
        return next(context);
    }

    public Task OnClosed(ConnectionContext connection)
    {
        return Task.CompletedTask;
    }

    public Task OnIdle(ConnectionContext connection)
    {
        return Task.CompletedTask;
    }

    // todo to be optimized
    private void Log(string method, MemorySequence<byte> buffer)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }


        var builder = new StringBuilder();
        builder.Append(method);
        builder.Append('[');
        builder.Append(buffer.Length);
        builder.Append(']');

        if (buffer.Length > 0)
        {
            builder.AppendLine();
        }

        var charBuilder = new StringBuilder();

        // Write the hex

        int i = 0;
        var e = buffer.GetEnumerator();
        while (e.MoveNext())
        {
            var e2 = e.Current.Span.GetEnumerator();
            while (e2.MoveNext())
            {
                byte b = e2.Current;

                builder.Append(b.ToString("X2", CultureInfo.InvariantCulture));
                builder.Append(' ');

                var bufferChar = (char)b;
                if (char.IsControl(bufferChar))
                {
                    charBuilder.Append('.');
                }
                else
                {
                    charBuilder.Append(bufferChar);
                }

                if ((i + 1) % 16 == 0)
                {
                    builder.Append("  ");
                    builder.Append(charBuilder);
                    if (i != buffer.Length - 1)
                    {
                        builder.AppendLine();
                    }

                    charBuilder.Clear();
                }
                else if ((i + 1) % 8 == 0)
                {
                    builder.Append(' ');
                    charBuilder.Append(' ');
                }

                i++;
            }
        }

        e.Dispose();


        // Different than charBuffer.Length since charBuffer contains an extra " " after the 8th byte.
        var numBytesInLastLine = buffer.Length % 16;

        if (numBytesInLastLine > 0)
        {
            // 2 (between hex and char blocks) + num bytes left (3 per byte)
            var padLength = 2 + (3 * (16 - numBytesInLastLine));
            // extra for space after 8th byte
            if (numBytesInLastLine < 8)
            {
                padLength++;
            }

            builder.Append(new string(' ', (int)padLength));
            builder.Append(charBuilder);
        }

        if (_options.Logging && _logger.IsEnabled(_options.LogLevel))
        {
            _logger.Log(_options.LogLevel, builder.ToString());
        }
    }


    private void Log(string method, ReadOnlySequence<byte> buffer)
    {
        if (!_logger.IsEnabled(LogLevel.Debug))
        {
            return;
        }

        var reader = new SequenceReader<byte>(buffer);
        var length = reader.Length;

        var builder = new StringBuilder();
        builder.Append(method);
        builder.Append('[');
        builder.Append(buffer.Length);
        builder.Append(']');

        if (buffer.Length > 0)
        {
            builder.AppendLine();
        }

        var charBuilder = new StringBuilder();

        // Write the hex

        int i = 0;
        while (!reader.End)
        {
            reader.TryRead(out byte b);
            builder.Append(b.ToString("X2", CultureInfo.InvariantCulture));
            builder.Append(' ');

            var bufferChar = (char)b;
            if (char.IsControl(bufferChar))
            {
                charBuilder.Append('.');
            }
            else
            {
                charBuilder.Append(bufferChar);
            }

            if ((i + 1) % 16 == 0)
            {
                builder.Append("  ");
                builder.Append(charBuilder);
                if (i != buffer.Length - 1)
                {
                    builder.AppendLine();
                }

                charBuilder.Clear();
            }
            else if ((i + 1) % 8 == 0)
            {
                builder.Append(' ');
                charBuilder.Append(' ');
            }

            i++;
        }


        // Different than charBuffer.Length since charBuffer contains an extra " " after the 8th byte.
        var numBytesInLastLine = buffer.Length % 16;

        if (numBytesInLastLine > 0)
        {
            // 2 (between hex and char blocks) + num bytes left (3 per byte)
            var padLength = 2 + (3 * (16 - numBytesInLastLine));
            // extra for space after 8th byte
            if (numBytesInLastLine < 8)
            {
                padLength++;
            }

            builder.Append(new string(' ', (int)padLength));
            builder.Append(charBuilder);
        }

        reader.Rewind(length);

        if (_options.Logging && _logger.IsEnabled(_options.LogLevel))
        {
            _logger.Log(_options.LogLevel, builder.ToString());
        }
    }


    public void Dispose()
    {
    }
}