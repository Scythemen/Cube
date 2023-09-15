using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;

namespace QuickSocket.Sample
{
    public class MiddlewareBoy : MiddlewareApple
    {
        public MiddlewareBoy(ILogger<MiddlewareBoy> logger) : base(logger)
        {
            base.Word = "boy";
        }
    }
}