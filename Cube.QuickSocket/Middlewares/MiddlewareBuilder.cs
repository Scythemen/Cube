using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Cube.QuickSocket;

public class MiddlewareBuilder : IDisposable
{
    private IList<IMiddleware> _middlewares = new List<IMiddleware>();
    public IList<IMiddleware> Middlewares => _middlewares;

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public MiddlewareBuilder(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger<MiddlewareBuilder>();
    }


    public MiddlewareBuilder Use<T>() where T : IMiddleware
    {
        IMiddleware instance = null;
        var list = _serviceProvider.GetServices<T>();
        foreach (var item in list)
        {
            if (item.GetType() == typeof(T))
            {
                instance = item;
                break;
            }
        }

        if (instance == null)
        {
            throw new ArgumentException($"{typeof(T)} not found in ServiceProvider");
        }

        _middlewares.Add(instance);
        return this;
    }

    public MiddlewareBuilder Use(IMiddleware middleware)
    {
        _middlewares.Add(middleware);
        return this;
    }

    public MiddlewareBuilder Clear()
    {
        _middlewares.Clear();
        return this;
    }

    public DecoderMiddlewareDelegate BuildDecoder()
    {
        if (_middlewares.Count == 0)
        {
            _logger.LogDebug("No middleware was set before build.");
            return null;
        }

        var delegates = new List<Func<DecoderMiddlewareDelegate, DecoderMiddlewareDelegate>>();
        var enumerator = _middlewares.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var m = enumerator.Current;

            DecoderMiddlewareDelegate dlg(DecoderMiddlewareDelegate next)
            {
                return async (ctx) => { await m.DecodeAsync(next, ctx); };
            }

            delegates.Add(dlg);
        }

        enumerator.Dispose();

        DecoderMiddlewareDelegate last = (context) => Task.CompletedTask;
        for (int i = delegates.Count - 1; i >= 0; i--)
        {
            last = delegates[i](last);
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var i = "socket -> " + string.Join(" -> ", _middlewares.Select(x => x.GetType().Name));
            _logger.LogDebug("Build decoder middleware delegates: {}", i);
        }

        return last;
    }

    public EncoderMiddlewareDelegate BuildEncoder()
    {
        if (_middlewares.Count == 0)
        {
            _logger.LogDebug("No middleware was set, return null.");
            return null;
        }

        var delegates = new List<Func<EncoderMiddlewareDelegate, EncoderMiddlewareDelegate>>();
        var enumerator = _middlewares.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var m = enumerator.Current;

            EncoderMiddlewareDelegate dlg(EncoderMiddlewareDelegate next)
            {
                return async (ctx) => { await m.EncodeAsync(next, ctx); };
            }

            delegates.Add(dlg);
        }

        enumerator.Dispose();

        EncoderMiddlewareDelegate last = (context) => Task.CompletedTask;
        for (int i = 0; i < delegates.Count; i++)
        {
            last = delegates[i](last);
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var i = "socket <- " + string.Join(" <- ", _middlewares.Select(x => x.GetType().Name));
            _logger.LogDebug("Build encoder middleware delegate: {}", i);
        }

        return last;
    }

    public KeyValuePair<Type, EncoderMiddlewareDelegate>[] BuildLadderEncoders()
    {
        return MiddlewareBuilder.BuildLadderEncoders(this.Middlewares, null);
    }

    public MiddlewareFeature BuildAsMiddlewareFeature()
    {
        var feature = new MiddlewareFeature(
            this.Middlewares,
            this.BuildDecoder(),
            this.BuildEncoder(),
            this.BuildLadderEncoders());
        return feature;
    }

    private static EncoderMiddlewareDelegate BuildEncoder(IList<IMiddleware> middlewareInstances, ILogger logger = null)
    {
        logger ??= NullLogger.Instance;

        var delegates = new List<Func<EncoderMiddlewareDelegate, EncoderMiddlewareDelegate>>();
        var enumerator = middlewareInstances.GetEnumerator();
        while (enumerator.MoveNext())
        {
            var m = enumerator.Current;

            EncoderMiddlewareDelegate dlg(EncoderMiddlewareDelegate next)
            {
                return async (ctx) => { await m.EncodeAsync(next, ctx); };
            }

            delegates.Add(dlg);
        }

        enumerator.Dispose();

        EncoderMiddlewareDelegate last = (context) => Task.CompletedTask;
        for (int i = 0; i < delegates.Count; i++)
        {
            last = delegates[i](last);
        }

        if (logger.IsEnabled(LogLevel.Debug))
        {
            var i = string.Join(" <- ", middlewareInstances.Select(x => x.GetType().Name));
            logger.LogDebug("Build encoder middleware delegate: {}", i);
        }

        return last;
    }

    public static KeyValuePair<Type, EncoderMiddlewareDelegate>[] BuildLadderEncoders(IList<IMiddleware> middlewareInstances, ILogger logger = null)
    {
        if (middlewareInstances == null || middlewareInstances.Count() == 0)
        {
            return null;
        }

        var result = new List<KeyValuePair<Type, EncoderMiddlewareDelegate>>();
        var temp = new List<IMiddleware>();
        foreach (var middleware in middlewareInstances)
        {
            temp.Add(middleware);
            var dele = MiddlewareBuilder.BuildEncoder(temp, logger);
            result.Add(new KeyValuePair<Type, EncoderMiddlewareDelegate>(middleware.GetType(), dele));
        }

        return result.ToArray();
    }

    public void Dispose()
    {
        _middlewares = null;
    }
}