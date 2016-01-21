namespace QuicDotNet
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Sockets;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using QuicDotNet.Frames;
    using QuicDotNet.Messages;
    using QuicDotNet.Packets;

    public class QuicClient : IDisposable
    {
        public const string QUIC_VERSION = "Q025";

        private bool versionAgreed;

        [CanBeNull]
        private UdpClient _udpClient;

        private bool disposed;

        [CanBeNull]
        private Task _receiveTask;

        public async Task ConnectAsync(string hostname, int port, CancellationToken cancellationToken = default(CancellationToken))
        {
            this._udpClient = new UdpClient(hostname, port);

            // Start listening
            this._receiveTask = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var received = await this._udpClient.ReceiveAsync();
                    Debug.WriteLine($"Received {received.Buffer.Length} bytes from {received.RemoteEndPoint}");
                }
            }, cancellationToken);

            // Setup default destination of client
            this._udpClient.Connect(hostname, port);


            // Generate random connection ID
            var random = new Random(Environment.TickCount);
            var buffer = new byte[sizeof(long)];
            random.NextBytes(buffer);

            var connectionId = BitConverter.ToUInt64(buffer, 0);
            var regularPacket = new RegularPacket(connectionId, 1, null);
            regularPacket.AddFrame(new StreamFrame(new ClientHandshakeMessage
                                                   {
                                                       { MessageTags.PAD, Enumerable.Repeat((byte)0x2d, 666).ToArray() },
                                                       { MessageTags.SNI, hostname },
                                                       { MessageTags.VER, QUIC_VERSION },
                                                       { MessageTags.CCS, new byte[] { 0x7b, 0x26, 0xe9, 0xe7, 0xe4, 0x5c, 0x71, 0xff, 0x01, 0xe8, 0x81, 0x60, 0x92, 0x92, 0x1a, 0xe8 } },
                                                       { MessageTags.MSPC, 100 },
                                                       { MessageTags.UAID, $"QuicDotNet/{Assembly.GetExecutingAssembly().GetName().Version} {System.Environment.OSVersion}" },
                                                       { MessageTags.TCID, 0 },
                                                       { MessageTags.PDMD, "X509" },
                                                       { MessageTags.SRBF, 1048576 },
                                                       { MessageTags.ICSL, 30 },
                                                       { MessageTags.SCLS, 1 },
                                                       { MessageTags.COPT, 1146636614 },
                                                       { MessageTags.IRTT, 9248 },
                                                       { MessageTags.CFCW, 15728640 },
                                                       { MessageTags.SFCW, 6291456 }
                                                   }, false, 1, 0));

            var bytesToSend = regularPacket.PadAndNullEncrypt();

            await this._udpClient.SendAsync(bytesToSend, bytesToSend.Length);
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
                    if (this._receiveTask?.IsCompleted ?? false)
                        this._receiveTask?.Dispose();
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
