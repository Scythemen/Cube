using System.Buffers;
using Cube.Timer;
using Cube.Utility;
using Microsoft.AspNetCore.Connections;

namespace Cube.QuickSocket;

/// <summary>
/// Indicate how to send the output,
/// choose whether to pass the output through the middlewares in pipeline before sending.
/// </summary>
public enum SendOptions
{
    /// <summary>
    /// Send directly, to the socket.
    /// </summary>
    Directly,

    /// <summary>
    /// Start from the tail of pipeline,
    /// and pass the output through all of the encoder middlewares until to the socket.
    /// </summary>
    TailMiddleware,

    /// <summary>
    /// Start from the specific encoder middleware,
    /// and pass the output through the encoder middlewares until to the socket.
    /// </summary>
    SpecificMiddleware,
}

public static class ConnectionContextExtensions
{
    public static TimerTaskHandle AddTimerTask(this ConnectionContext context,
        int milliseconds, Action<object> action, object args)
    {
        return TcpHelper.Timer.AddTask(TimeSpan.FromMilliseconds(milliseconds), action, args);
    }


    public static TimerTaskHandle AddTimerTask(this ConnectionContext context,
        TimeSpan timeSpan, Action<object> action, object args)
    {
        return TcpHelper.Timer.AddTask(timeSpan, action, args);
    }


    public static void SetIdleTime(this ConnectionContext context, int idleMilliseconds)
    {
        SetIdleTime(context, TimeSpan.FromMilliseconds(idleMilliseconds));
    }


    public static void ResetIdleTime(this ConnectionContext context)
    {
        var feature = context.Features.Get<IdleStateFeature>();
        if (feature == null || feature.TimerTaskHandler == null)
        {
            return;
        }

        feature.TimerTaskHandler.Cancel();
        feature.TimerTaskHandler = TcpHelper.Timer.AddTask(TimeSpan.FromMilliseconds(feature.IdleMilliseconds), (connection) =>
        {
            if (connection is ConnectionContext ctx)
            {
                var middlewareFeature = ctx.Features.Get<MiddlewareFeature>();
                if (middlewareFeature != null)
                {
                    foreach (var middleware in middlewareFeature.Middlewares)
                    {
                        middleware.OnIdle(ctx);
                    }
                }
            }
        }, context);
    }

    public static void SetIdleTime(this ConnectionContext context, TimeSpan idleTime)
    {
        if (idleTime.TotalMilliseconds < 1)
        {
            idleTime = TimeSpan.FromMilliseconds(13);
        }

        var feature = context.Features.Get<IdleStateFeature>();
        if (feature == null)
        {
            feature = new IdleStateFeature() { };
            context.Features.Set(feature);
        }

        feature.IdleMilliseconds = (int)idleTime.TotalMilliseconds;
        feature.TimerTaskHandler?.Cancel();
        feature.TimerTaskHandler = TcpHelper.Timer.AddTask(idleTime, (connection) =>
        {
            if (connection is ConnectionContext ctx)
            {
                var middlewareFeature = ctx.Features.Get<MiddlewareFeature>();
                if (middlewareFeature != null)
                {
                    foreach (var middleware in middlewareFeature.Middlewares)
                    {
                        middleware.OnIdle(ctx);
                    }
                }
            }
        }, context);
    }


    public static void CancelIdleTime(this ConnectionContext context)
    {
        var feature = context.Features.Get<IdleStateFeature>();
        feature?.TimerTaskHandler?.Cancel();
    }

    public static async ValueTask<SendResult> Send(
        this ConnectionContext context,
        string hex,
        SendOptions sendOption = SendOptions.Directly,
        Type specificMiddleware = default)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return new SendResult() { Completed = false, Message = "Argument null or empty" };
        }

        var bytes = Convert.FromHexString(hex);
        return await context.Send(new MemorySequence<byte>(bytes), sendOption, specificMiddleware);
    }


    public static async ValueTask<SendResult> Send(
        this ConnectionContext context,
        byte[] bytes,
        SendOptions sendOption = SendOptions.Directly,
        Type specificMiddleware = default)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return new SendResult() { Completed = false, Message = "Argument null or empty" };
        }

        return await context.Send(new MemorySequence<byte>(bytes), sendOption, specificMiddleware);
    }


    public static async ValueTask<SendResult> Send(
        this ConnectionContext context,
        ReadOnlySequence<byte> sequence,
        SendOptions sendOption = SendOptions.Directly,
        Type specificMiddleware = default)
    {
        var result = new SendResult() { Completed = false };
        try
        {
            if (sendOption == SendOptions.Directly)
            {
                foreach (var m in sequence)
                {
                    await context.Transport.Output.WriteAsync(m);
                }

                result.Completed = true;
            }
            else
            {
                var middlewareFeature = context.Features.Get<MiddlewareFeature>();
                if (middlewareFeature == null)
                {
                    throw new NotSupportedException($"Fail to send with options:{sendOption}, middleware feature not found");
                }

                var memory = new MemorySequence<byte>();
                foreach (var m in sequence)
                {
                    memory.AddLast(m);
                }

                var encoderContext = new EncoderContext(context).SetOutput(memory);

                if (sendOption == SendOptions.TailMiddleware)
                {
                    await middlewareFeature.EncoderDelegate.Invoke(encoderContext);
                }

                if (sendOption == SendOptions.SpecificMiddleware)
                {
                    var deleg = middlewareFeature.SpecificEncoderDelegates
                        .Where(x => x.Key == specificMiddleware).FirstOrDefault().Value;

                    if (deleg == null)
                    {
                        throw new NotSupportedException($"Fail to send with options:{sendOption}, middleware {specificMiddleware} not found");
                    }

                    await deleg.Invoke(encoderContext);
                }

                if (!encoderContext.Output.IsEmpty)
                {
                    foreach (var m in encoderContext.Output)
                    {
                        await context.Transport.Output.WriteAsync(m);
                    }
                }

                result.Completed = true;
                context.ResetIdleTime();
            }
        }
        catch (Exception e)
        {
            result.Message = e.Message;
            result.Exception = e;
        }

        return result;
    }


    public static async ValueTask<SendResult> Send(
        this ConnectionContext context,
        MemorySequence<byte> memory,
        SendOptions sendOption = SendOptions.Directly,
        Type specificMiddleware = default)
    {
        var result = new SendResult() { Completed = false };
        try
        {
            if (sendOption == SendOptions.Directly)
            {
                foreach (var m in memory)
                {
                    await context.Transport.Output.WriteAsync(m);
                }

                result.Completed = true;
            }
            else
            {
                var middlewareFeature = context.Features.Get<MiddlewareFeature>();
                if (middlewareFeature == null)
                {
                    throw new NotSupportedException($"Fail to send with options:{sendOption}, middleware feature not found");
                }

                var encoderContext = new EncoderContext(context).SetOutput(memory);

                if (sendOption == SendOptions.TailMiddleware)
                {
                    await middlewareFeature.EncoderDelegate.Invoke(encoderContext);
                }

                if (sendOption == SendOptions.SpecificMiddleware)
                {
                    var deleg = middlewareFeature.SpecificEncoderDelegates
                        .Where(x => x.Key == specificMiddleware).FirstOrDefault().Value;

                    if (deleg == null)
                    {
                        throw new NotSupportedException($"Fail to send with options:{sendOption}, middleware {specificMiddleware} not found");
                    }

                    await deleg.Invoke(encoderContext);
                }

                if (!encoderContext.Output.IsEmpty)
                {
                    foreach (var m in encoderContext.Output)
                    {
                        await context.Transport.Output.WriteAsync(m);
                    }
                }

                result.Completed = true;
            }
        }
        catch (Exception e)
        {
            result.Message = e.Message;
            result.Exception = e;
        }

        return result;
    }
}