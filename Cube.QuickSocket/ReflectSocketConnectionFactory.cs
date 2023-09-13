// using System.Net;
// using Microsoft.AspNetCore.Connections;
// using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
// using Microsoft.Extensions.Options;
//
// namespace Cube.QuickSocket;
//
// public sealed class ReflectSocketConnectionFactory : IConnectionFactory, IAsyncDisposable
// {
//     private const string FactoryTypeName =
//         "Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets.SocketConnectionFactory";
//
//     private readonly IConnectionFactory _connectionFactory;
//
//     public ReflectSocketConnectionFactory(IOptions<SocketTransportOptions> options, ILoggerFactory loggerFactory)
//     {
//         ArgumentNullException.ThrowIfNull(options);
//         ArgumentNullException.ThrowIfNull(loggerFactory);
//
//         var assembly = typeof(SocketTransportOptions).Assembly;
//         var factoryType = assembly.GetType(FactoryTypeName);
//
//         ArgumentNullException.ThrowIfNull(factoryType);
//
//         _connectionFactory = Activator.CreateInstance(factoryType, options, loggerFactory) as IConnectionFactory;
//         if (_connectionFactory == null)
//         {
//             throw new NotSupportedException("Fail to create instance of SocketConnectionFactory");
//         }
//     }
//
//
//     public async ValueTask<ConnectionContext> ConnectAsync(EndPoint endpoint,
//         CancellationToken cancellationToken = default)
//     {
//         var ipEndPoint = endpoint as IPEndPoint;
//
//         if (ipEndPoint is null)
//         {
//             throw new NotSupportedException("The SocketConnectionFactory only supports IPEndPoints for now.");
//         }
//
//         return await _connectionFactory.ConnectAsync(ipEndPoint, cancellationToken);
//     }
//
//     public ValueTask DisposeAsync()
//     {
//         (_connectionFactory as IAsyncDisposable)?.DisposeAsync();
//         return default;
//     }
// }