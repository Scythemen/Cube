using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestCommon
{
    public class TcpOptions
    {
        public string Ip { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 9910;

        public int TotalConnections { get; set; } = 10;
        public int IncreaseRate { get; set; } = 2;
        public int IncreaseInterval { get; set; } = 1000; // ms

    }
}
