using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.IO.Pipelines;
using Cube.Utility;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace Cube.QuickSocket.Sample
{
    public class MiddlewareApple : IMiddleware
    {
        private readonly ILogger _logger;

        internal string Word = "apple";
        internal int IdleMilliseconds = 13 * 1000;

        public MiddlewareApple(ILogger<MiddlewareApple> logger)
        {
            _logger = logger;
        }

        public Task OnConnected(ConnectionContext connection)
        {
            _logger.LogDebug("OnConnected");

            // set idle time, if timeout OnIdle event will be fired.
            connection.SetIdleTime(IdleMilliseconds);
            _logger.LogDebug("set connection idle time milliseconds: {}", IdleMilliseconds);

            return Task.CompletedTask;
        }

        public Task DecodeAsync(DecoderMiddlewareDelegate next, DecoderContext context)
        {
            _logger.LogDebug("InvokeAsync");

            if (context.Input.IsEmpty)
            {
                return Task.CompletedTask;
            }

            var wordBytes = System.Text.Encoding.ASCII.GetBytes(Word);

            var position = context.Input.FirstOf(wordBytes, true);

            _logger.LogDebug("try to find the first '{}', position={} ", Word, position?.GetInteger());

            if (position != null)
            {
                context.Input = context.Input.Slice(position.Value);
                _logger.LogDebug("after slice : {} ", context.Input.ToHex());
            }

            return next(context);
        }

        public Task EncodeAsync(EncoderMiddlewareDelegate next, EncoderContext context)
        {
            // write head
            var wordBytes = System.Text.Encoding.ASCII.GetBytes(Word);
            context.Output.AddFirst(wordBytes);

            _logger.LogDebug("write head {}, : {} ", Word, context.Output.ToHex());

            return next(context);
        }


        public Task OnClosed(ConnectionContext connection)
        {
            _logger.LogDebug("OnClosed");
            return Task.CompletedTask;
        }

        public Task OnIdle(ConnectionContext connection)
        {
            _logger.LogDebug("OnIdle");

            _logger.LogDebug("close connection because idle {} milliseconds", IdleMilliseconds);
            connection?.Abort();

            return Task.CompletedTask;
        }


        public void Dispose()
        {
        }
    }
}