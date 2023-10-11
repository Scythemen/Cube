using Cube.QuickSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;


namespace QuickSocket.UnitTest
{
    internal class TestTcpSocket
    {
        private ILogger logger;

        [SetUp]
        public void Setup()
        {
            logger = LoggerInitializer.InitLogger<TestBytesHelper>();
        }

        [Test]
        public void NoDependencyInjectionTcpServer()
        {
            var ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9910);

            var svr = new Cube.QuickSocket.TcpServer()
                .StartAsync(ip)
                .GetAwaiter().GetResult();

            logger.LogInformation("server started");

            svr.StopAsync().GetAwaiter().GetResult();

            logger.LogInformation("server stop");

            svr.StartAsync(ip).GetAwaiter().GetResult();

            logger.LogInformation("server started again");

            svr.StopAsync().GetAwaiter().GetResult();

            logger.LogInformation("server stop again");

        }


        [Test]
        public void TestMiddlewareBuilder_instance()
        {
            var ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9910);

            var svr = new Cube.QuickSocket.TcpServer()
                .UseMiddleware(new LoggingMiddleware())
                .UseMiddleware(null)  // <-- will fail
                .UseMiddleware<FallbackMiddleware>() // <-- will fail
                .StartAsync(ip)
                .GetAwaiter().GetResult();

        }


        [Test]
        public void DependencyInjectionTcpServer()
        {
            var ip = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9910);

            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder.ClearProviders();
                    builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .Configure<FallbackMiddlewareOptions>(config.GetSection(nameof(FallbackMiddlewareOptions)))
                .Configure<IConfiguration>(config)
                .AddSingleton<FlowAnalyzeMiddleware>()
                .AddSingleton<FallbackMiddleware>()
                .AddSingleton<LoggingMiddleware>()
                .AddSingleton(sp => ActivatorUtilities.CreateInstance<TcpServer>(sp)) //  <------------ not addSingleton<TcpServer>()
                .BuildServiceProvider();

            //--- get tcp-server from service provider

            var server = serviceProvider.GetRequiredService<TcpServer>();
            server.StartAsync(ip).GetAwaiter().GetResult();
            logger.LogInformation("server started");

            server.StopAsync().GetAwaiter().GetResult();

            logger.LogInformation("server stop");


            // init manually
            var svr = new TcpServer(serviceProvider)
                .UseMiddleware<LoggingMiddleware>()
                .UseMiddleware<FlowAnalyzeMiddleware>()
                .UseMiddleware<FallbackMiddleware>()
                .StartAsync(ip).GetAwaiter().GetResult();

            logger.LogInformation("manually: server started");

            svr.StopAsync().GetAwaiter().GetResult();

            logger.LogInformation("manually: server stop");

        }

    }
}
