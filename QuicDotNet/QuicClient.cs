namespace QuicDotNet
{
    using System;
    using System.Diagnostics;
    using System.Net.Sockets;

    using QuicDotNet.Frames;
    using QuicDotNet.Packets;

    public class QuicClient
    {
        public const uint QUIC_VERSION = 81485053;

        private bool versionAgreed;

        private UdpClient _udpClient;

        public QuicClient()
        {
            this._udpClient = new UdpClient();
        }

        public QuicClient(AddressFamily family)
        {
            this._udpClient = new UdpClient(family);
        }

        public QuicClient(int port)
        {
            this._udpClient = new UdpClient(port);
        }

        public QuicClient(System.Net.IPEndPoint localEp)
        {
            this._udpClient = new UdpClient(localEp);
        }

        public QuicClient(int port, AddressFamily family)
        {
            this._udpClient = new UdpClient(port, family);
        }

        public QuicClient(string hostname, int port)
        {
            this._udpClient = new UdpClient(hostname, port);
        }

        public void Connect(string hostname, int port)
        {
            // Setup default destination of client
            this._udpClient.Connect(hostname, port);

            var random = new Random(Environment.TickCount);
            var connectionId = Convert.ToUInt64(random.Next(1000, int.MaxValue));
            var regularPacket = new RegularPacket(connectionId, 1, null);
            regularPacket.AddFrame(new StreamFrame(1, 0));
        }
    }
}
