namespace QuicDotNet
{
    using System;
    using System.Net.Sockets;

    using QuicDotNet.Frames;
    using QuicDotNet.Packets;

    public class QuicClient : IDisposable
    {
        public const string QUIC_VERSION = "Q025";

        private bool versionAgreed;

        private readonly UdpClient _udpClient;

        private bool disposed;

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

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    ((IDisposable)this._udpClient).Dispose();
                }

                // There are no unmanaged resources to release, but
                // if we add them, they need to be released here.
            }
            this.disposed = true;

            // If it is available, make the call to the
            // base class's Dispose(Boolean) method
            //base.Dispose(disposing);
        }
    }
}
