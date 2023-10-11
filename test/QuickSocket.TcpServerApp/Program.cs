using Microsoft.Extensions.Configuration;
using System.Net;
using Cube.QuickSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QuickSocket.TcpServerApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    // builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .Configure<FallbackMiddlewareOptions>(config.GetSection(nameof(FallbackMiddlewareOptions)))
                .Configure<FlowAnalyzeMiddlewareOptions>(config.GetSection(nameof(FlowAnalyzeMiddlewareOptions)))
                .Configure<IConfiguration>(config)
                .AddSingleton(sp => ActivatorUtilities.CreateInstance<TcpServer>(sp)) //  <------------ not addSingleton<TcpServer>()
                .AddSingleton<FlowAnalyzeMiddleware>()
                .AddSingleton<FallbackMiddleware>()
                .BuildServiceProvider();


            var options = config.GetSection(nameof(TcpOptions)).Get<TcpOptions>();

            var ip = IPAddress.Parse(options.Ip);
            var addr = new IPEndPoint(ip, options.Port);


            // var stopToken = new CancellationTokenSource();

            var server = await serviceProvider.GetRequiredService<TcpServer>()
                .UseMiddleware<FlowAnalyzeMiddleware>()
                .UseMiddleware<FallbackMiddleware>()
                .StartAsync(addr);


            Console.WriteLine(" ............ ctrl+c to exit...........!");

            var tcs = new TaskCompletionSource<object>();
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                //  stopToken.Cancel();
                tcs.TrySetResult(null);
            };
            await tcs.Task;

            //   await server.StopAsync();
            server.Dispose();

            Console.WriteLine("         << exit! ");
        }
    }
}