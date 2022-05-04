using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TestServer
{
    public class ConnectionInfo
    {
        public IPAddress groupAddress { get; set; }
        public int localPort { get; set; }
        public int remotePort { get; set; }
        public int ttl { get; set; }
        public UdpClient udpClient { get; set; }
        public IPEndPoint remoteEndPoint { get; set; }
    }
}
