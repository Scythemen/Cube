using Microsoft.Extensions.Configuration;
using Cube.QuickSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace QuickSocket.TcpClientApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    //  builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                    builder.AddConsole();
                    builder.AddDebug();
                })
                .Configure<IConfiguration>(configuration)
                .Configure<TcpOptions>(configuration.GetSection(nameof(TcpOptions)))
                .Configure<FlowAnalyzeMiddlewareOptions>(configuration.GetSection(nameof(FlowAnalyzeMiddlewareOptions)))
                .AddSingleton<ConcurrentClients>()
                .AddScoped<FlowAnalyzeMiddleware>()
                .AddSingleton<FallbackMiddleware>()
                .BuildServiceProvider();

            var stopToken = new CancellationTokenSource();

            var clients = serviceProvider.GetService<ConcurrentClients>();
            clients.StartAsync(stopToken);


            Console.WriteLine(" ............ ctrl+c to exit...........!");

            var tcs = new TaskCompletionSource<object>();
            Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                tcs.TrySetResult(null);
                stopToken.Cancel();
            };
            await tcs.Task;

            clients.Stop();

            Console.WriteLine("           << exit ");
        }
    }
}