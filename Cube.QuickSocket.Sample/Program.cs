using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;

namespace Cube.QuickSocket.Sample
{
    internal static partial class Program
    {
        static async Task Main(string[] args)
        {
            var ip = new IPEndPoint(IPAddress.Any, 9910);

            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    builder.AddNLog();
                })
                .Configure<FallbackMiddlewareOptions>(config.GetSection(nameof(FallbackMiddlewareOptions)))
                .Configure<IConfiguration>(config)
                .AddSingleton<FlowAnalyzeMiddleware>()
                .AddSingleton<FallbackMiddleware>()
                .AddSingleton<LoggingMiddleware>()
                .AddSingleton<MiddlewareCat>()
                .AddSingleton<MiddlewareApple>()
                .AddSingleton<MiddlewareBoy>()
                .BuildServiceProvider();


            var stopToken = new CancellationTokenSource();

            //--- tcp-server
            var server = await new TcpServer(serviceProvider)
                .UseMiddleware<LoggingMiddleware>()
                .UseMiddleware<FlowAnalyzeMiddleware>()
                .UseMiddleware<MiddlewareApple>()
                .UseMiddleware<MiddlewareBoy>()
                .UseMiddleware<MiddlewareCat>()
                .UseMiddleware<FallbackMiddleware>()
                .StartAsync(ip);

            
            //----------- tcp-client 
            
            await Task.Delay(2000);
            
            var client = await new TcpClient(serviceProvider)
                .UseMiddleware<MiddlewareApple>()
                .UseMiddleware<MiddlewareCat>()
                .ConnectAsync(ip);
            
            var bytes = System.Text.Encoding.ASCII.GetBytes("hello world");
            await client.ConnectionContext.Send(bytes);
            
            await Task.Delay(2000);
            bytes = System.Text.Encoding.ASCII.GetBytes("big bang");
            await client.ConnectionContext.Send(bytes);
            //------------

            var tcs = new TaskCompletionSource<object>();
            Console.CancelKeyPress += (sender, e) =>
            {
                stopToken.Cancel();
                tcs.TrySetResult(null);
                e.Cancel = true;
            };
            await tcs.Task;
        }
    }
}