using System.Collections.Concurrent;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Cube.QuickSocket;

public class FlowAnalyzeMiddleware : IMiddleware
{
    private record QueueItem(long Tick, int Bytes);

    private readonly ILogger _logger;
    private readonly FlowAnalyzeMiddlewareOptions _options;
    private readonly ConcurrentQueue<QueueItem> _inputQueue = new ConcurrentQueue<QueueItem>();
    private readonly ConcurrentQueue<QueueItem> _outputQueue = new ConcurrentQueue<QueueItem>();
    private readonly FlowAnalyzeFeature _flowAnalyzeFeature = new FlowAnalyzeFeature();
    private long _totalInputBytes = 0;
    private long _totalOutputBytes = 0;
    private long _connections = 0;
    private int _inputQueueLocker = 0;
    private int _outputQueueLocker = 0;

    public long TotalInputBytes => _totalInputBytes;
    public long TotalOutputBytes => _totalOutputBytes;
    public long Connections => _connections;
    public double InputRate => GetInputRate();
    public double OutputRate => GetOutputRate();


    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    public FlowAnalyzeMiddleware(
        ILogger<FlowAnalyzeMiddleware> logger,
        IOptions<FlowAnalyzeMiddlewareOptions> options = null)
    {
        _logger = logger == null ? NullLogger.Instance : logger;
        _options = options == null ? new FlowAnalyzeMiddlewareOptions() : options.Value;

        _ = UpdateAsync();
    }

    public Task OnConnected(ConnectionContext connection)
    {
        connection.Features.Set(_flowAnalyzeFeature);

        Interlocked.Increment(ref _connections);

        return Task.CompletedTask;
    }

    public async Task DecodeAsync(DecoderMiddlewareDelegate next, DecoderContext context)
    {
        var start = context.Input.Length;

        await next(context);

        var consumed = start - context.Input.Length;
        if (consumed > 0)
        {
            Interlocked.Add(ref _totalInputBytes, consumed);
            TrimQueue(_inputQueue, ref _inputQueueLocker);
            _inputQueue.Enqueue(new QueueItem(Environment.TickCount64, (int)consumed));
        }
    }

    public Task EncodeAsync(EncoderMiddlewareDelegate next, EncoderContext context)
    {
        if (context.Output.Length > 0)
        {
            Interlocked.Add(ref _totalOutputBytes, context.Output.Length);
            TrimQueue(_outputQueue, ref _outputQueueLocker);
            _outputQueue.Enqueue(new QueueItem(Environment.TickCount64, context.Output.Length));
        }

        return next(context);
    }

    public Task OnClosed(ConnectionContext connection)
    {
        Interlocked.Decrement(ref _connections);
        return Task.CompletedTask;
    }

    public Task OnIdle(ConnectionContext connection)
    {
        return Task.CompletedTask;
    }

    private double GetInputRate()
    {
        TrimQueue(_inputQueue, ref _inputQueueLocker);

        // when the trimming operation is starting by other thread and is not completed,
        // the summing value could be an approximation.
        return _inputQueue.Sum(item => item.Bytes) * 1.0D / _options.Interval;
    }

    private double GetOutputRate()
    {
        TrimQueue(_outputQueue, ref _outputQueueLocker);

        // when the trimming operation is starting by other thread and is not completed,
        // the summing value could be an approximation.
        return _outputQueue.Sum(item => item.Bytes) * 1.0D / _options.Interval;
    }

    private void TrimQueue(ConcurrentQueue<QueueItem> queue, ref int locker)
    {
        if (Interlocked.CompareExchange(ref locker, 1, 0) == 0)
        {
        }
        else
        {
            return;
        }

        var lastTick = Environment.TickCount64 - _options.Interval * 1000;
        while (queue.TryPeek(out var item))
        {
            if (item.Tick < lastTick)
            {
                queue.TryDequeue(out _);
            }
            else
            {
                break;
            }
        }

        Interlocked.Exchange(ref locker, 0);
    }


    private async Task UpdateAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            await Task.Delay(1000 * _options.Interval, _cancellationTokenSource.Token);

            _flowAnalyzeFeature.TotalInputBytes = _totalInputBytes;
            _flowAnalyzeFeature.TotalOutputBytes = _totalOutputBytes;
            _flowAnalyzeFeature.Connections = _connections;
            _flowAnalyzeFeature.InputRate = GetInputRate();
            _flowAnalyzeFeature.OutputRate = GetOutputRate();

            if (_options.Logging && _logger.IsEnabled(_options.LogLevel))
            {
                _logger.Log(_options.LogLevel, "FlowAnalyze: {}", _flowAnalyzeFeature);
            }
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _inputQueue.Clear();
        _outputQueue.Clear();
    }
}