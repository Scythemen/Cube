using System.Buffers;
using System.Net;
using System.Text;
using Cube.QuickSocket;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TestCommon;

namespace TestTcpClient
{
    public class ConcurrentClients
    {
        private CancellationTokenSource _cancellationToken;
        private readonly IServiceProvider _serviceProvider;
        private readonly TcpOptions _options;
        private readonly ILogger _logger;

        public ConcurrentClients(
            IServiceProvider serviceProvider,
            ILogger<ConcurrentClients> logger,
            IOptions<TcpOptions> options)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _options = options.Value;
        }

        public async Task StartAsync(CancellationTokenSource cancellationTokenSource = null)
        {
            _cancellationToken = cancellationTokenSource ?? new CancellationTokenSource();

            StringBuilder str = new StringBuilder();
            str.Append(Environment.NewLine);
            str.AppendFormat(">>> Concurrent clients starting...\r\n");
            str.AppendFormat(">>> Connect to: {0}:{1} \r\n", _options.Ip, _options.Port);
            str.AppendFormat(">>> TotalConnections : {0} \r\n", _options.TotalConnections);
            str.AppendFormat(">>> IncreaseRate : {0} \r\n", _options.IncreaseRate);
            str.AppendFormat(">>> IncreaseInterval : {0} \r\n", _options.IncreaseInterval);
            str.AppendFormat(Environment.NewLine + "Will Start in 5s..." + Environment.NewLine);

            _logger.LogInformation(str.ToString());

            await Task.Delay(5000, _cancellationToken.Token).ConfigureAwait(false);

            var ip = IPAddress.Parse(_options.Ip);
            var addr = new IPEndPoint(ip, _options.Port);

            var waitList = new List<Task>();

            await Task.Run(async () =>
            {
                for (int i = 0; i < _options.TotalConnections; i++)
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var t2 = StartClientAsync(i, addr);
                    waitList.Add(t2);

                    if (i % _options.IncreaseRate == 0)
                    {
                        await Task.Delay(_options.IncreaseInterval, _cancellationToken.Token).ConfigureAwait(false);
                    }
                }
            }, _cancellationToken.Token);


            _logger.LogInformation("The task of creating connections has finished. loop to send message....");

            //Task.WaitAll(waitList.ToArray());
            // _logger.LogInformation("exit !");
        }

        private async Task StartClientAsync(int index, IPEndPoint ip)
        {
            ConnectionContext ctx = null;
            try
            {
                var tcp = await new Cube.QuickSocket.TcpClient(_serviceProvider)
                    .UseMiddleware<FlowAnalyzeMiddleware>()
                    .UseMiddleware<FallbackMiddleware>()
                    .ConnectAsync(ip);

                ctx = tcp.ConnectionContext;

                await SendMsgLoop(tcp);
            }
            catch (Exception e)
            {
                _logger.LogDebug("Client {}, id:{}, R:{}, L:{}, error: {}",
                    index, ctx?.ConnectionId, ctx?.RemoteEndPoint, ctx?.LocalEndPoint, e.Message);
            }
        }

        private async Task SendMsgLoop(TcpClient client)
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                byte[] sendBytes = null;
                try
                {
                    //  var arr = System.Text.ASCIIEncoding.ASCII.GetBytes("hello world");
                    var rand = new Random((int)(DateTime.Now.Millisecond));
                    var len = rand.Next(8, 128);
                    sendBytes = ArrayPool<byte>.Shared.Rent(len);
                    for (int i = 0; i < sendBytes.Length; i++)
                    {
                        sendBytes[i] = (byte)i;
                    }

                    await client.ConnectionContext.Send(sendBytes);

                    ArrayPool<byte>.Shared.Return(sendBytes);

                    // delay & wait to next loop
                    var next = Random.Shared.Next(10, 80);
                    await Task.Delay(next * 1000, _cancellationToken.Token).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    if (sendBytes != null)
                    {
                        ArrayPool<byte>.Shared.Return(sendBytes);
                    }

                    break;
                }
            }
        }


        public void Stop()
        {
            _cancellationToken.Cancel();
        }
    }
}