namespace QuicDotNet
{
    using System;
    using System.IO;

    using JetBrains.Annotations;

    /// <summary>
    /// A single transfer that is queued for transfer through a <see cref="MultiplexingManager"/>
    /// </summary>
    public class MultiplexedTransfer
    {
        public MultiplexedTransfer(int priority, Stream stream, uint streamId, bool terminateStream = true)
        {
            this.Priority = priority;
            this.Stream = stream;
            this.StreamId = streamId;
            this.TerminateStream = terminateStream;
        }

        public string TransferId { get; } = Guid.NewGuid().ToString("N");

        public int Priority { get; set; }

        [NotNull]
        public Stream Stream { get; private set; }

        /// <summary>
        /// To avoid stream ID collision, the Stream-ID must be
        /// even if the server initiates the stream, and odd if the client
        /// initiates the stream. 0 is not a valid Stream-ID.Stream 1 is
        /// reserved for the crypto handshake, which should be the first client-
        /// initiated stream.
        /// </summary>
        public uint StreamId { get; private set; }

        /// <summary>
        /// Gets a value indiciating whether the stream will be automatically closed when the <see cref="MultiplexedTransfer.Stream"/> is fully transmitted.
        /// </summary>
        public bool TerminateStream { get; private set; }
    }
}