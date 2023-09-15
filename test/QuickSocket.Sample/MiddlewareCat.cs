using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace QuickSocket.Sample
{
    public class MiddlewareCat : MiddlewareApple
    {
        public MiddlewareCat(ILogger<MiddlewareCat> logger) : base(logger)
        {
            base.Word = "cat";
        }
    }
}