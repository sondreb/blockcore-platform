using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Blockcore.Platform.Networking
{
    public class NetworkClient
    {
        public NetworkClient(TcpClient tcpClient)
        {
            this.TcpClient = tcpClient;
        }

        public NetworkClient(TcpClient tcpClient, string clientIP) : this(tcpClient)
        {
            this.ClientIP = clientIP;
        }

        public TcpClient TcpClient { get; set; }

        /// <summary>
        /// Used for querying the hub without actually performing a TCP connection.
        /// </summary>
        public string ClientIP { get; set; }
    }
}
