using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace QuickSocket.UnitTest;

public class LoggerInitializer
{
    public static ILoggerFactory InitFactory()
    {
        var consoleTarget = new NLog.Targets.ConsoleTarget("consoleTarget");
        consoleTarget.Layout =
            @"${date:format=HH\:mm\:ss.fff}|${uppercase:${level}}|${logger}|threadId ${threadid}|${message} ${exception:format=tostring}";

        consoleTarget.Layout = @"${date:format=HH\:mm\:ss,fff}|${level:uppercase=true:padding=-5}|${message}";

        var fileTarget = new NLog.Targets.FileTarget("fileTarget");
        fileTarget.Layout = consoleTarget.Layout;
        fileTarget.FileName = "./test.log";

        var config = new NLog.Config.LoggingConfiguration();
        config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, consoleTarget, "*");
        config.AddRule(NLog.LogLevel.Trace, NLog.LogLevel.Fatal, fileTarget, "*");

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.ClearProviders();
            builder.AddNLog(config);
        });

        return loggerFactory;
    }

    public static ILogger InitLogger<TClass>()
    {
        var factory = InitFactory();
        return factory.CreateLogger<TClass>();
    }
}