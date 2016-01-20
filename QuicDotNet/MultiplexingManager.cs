using System.IO;

namespace QuicDotNet
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using QuicDotNet.Frames;

    /// <summary>
    /// The multiplexing manager handles the generation of regular packets given queued byte streams that are prioritized for transmission
    /// </summary>
    public class MultiplexingManager : ConcurrentQueue<MultiplexedTransfer>
    {
        private readonly ulong? _connectionid;

        private readonly bool _isServer;

        private readonly object _packetNumberLock = new object();
        private ulong _packetNumber;

        private readonly object _streamIdLock = new object();
        private uint _currentStreamId;

        private readonly object _dequeueLock = new object();
        
        /// <summary>
        /// Creates a new multiplexing manager.  If this multiplexing manager is for a SERVER, then <paramref name="connectionId"/> is null and will be set when received from the client.
        /// </summary>
        /// <param name="connectionId"></param>
        public MultiplexingManager(ulong? connectionId)
        {
            this._connectionid = connectionId;
            this._isServer = !connectionId.HasValue;
            this._currentStreamId = this._isServer ? 2U : 1U;
        }

        public void SendBytes(byte[] bytes, uint? specificStreamId = null)
        {
            this.SendFile(new MemoryStream(bytes), specificStreamId);
        }

        public void SendFile(Stream stream, uint? specificStreamId = null)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            uint streamId;

            if (specificStreamId.HasValue)
                streamId = specificStreamId.Value;
            else
                lock (this._streamIdLock)
                {
                    this._currentStreamId += 2U;
                    streamId = this._currentStreamId;
                }

            this.Enqueue(new MultiplexedTransfer(0, stream, streamId));
        }

        [NotNull, ItemCanBeNull]
        public async Task<Packets.RegularPacket> CutNextPacketAsync()
        {
            if (!this._connectionid.HasValue)
                throw new InvalidOperationException("Connection ID is not established!");

            if (this.Count == 0)
                return null;

            uint remaining = Packets.AbstractPacketBase.MTU;

            ulong packetNumber;
            lock (this._packetNumberLock)
            {
                this._packetNumber++;
                packetNumber = this._packetNumber;
            }

            var regular = new Packets.RegularPacket(this._connectionid.Value, packetNumber, null); // TODO: FEC GROUP's.

            remaining -= regular.GetHeaderLength();

            MultiplexedTransfer nextTransfer;
            long assignedDataSize;
            bool fin;
            StreamFrame streamFrame;
            lock (this._dequeueLock)
            {
                MultiplexedTransfer peekedTransfer;
                do
                {
                    if (!this.TryPeek(out peekedTransfer))
                        return null; // My queue is _now_ empty, just say nothing to do.

                    // Prototype our stream frame
                    var streamRemainingByteCount = peekedTransfer.Stream.Length - peekedTransfer.Stream.Position;

                    streamFrame = new StreamFrame(peekedTransfer.StreamId, Convert.ToUInt64(peekedTransfer.Stream.Position));
                    var prototypeLength = streamFrame.GetMetadataLength();

                    assignedDataSize = Math.Min(remaining, prototypeLength);
                    var transferDone = assignedDataSize == streamRemainingByteCount;
                    fin = transferDone && peekedTransfer.TerminateStream;

                    if (!this.TryDequeue(out nextTransfer))
                        return null; // My queue is _now_ empty, just say nothing to do.
                    if (nextTransfer.TransferId != peekedTransfer.TransferId)
                        this.Enqueue(nextTransfer); // Whoops, something changed outside of our lock... so, redo our calculations.

                }
                while (nextTransfer.TransferId != peekedTransfer.TransferId);
            }

            try
            {
                // Hydrate our stream frame prototype
                var streamData = new byte[assignedDataSize];
                await nextTransfer.Stream.ReadAsync(streamData, (int)nextTransfer.Stream.Position, streamData.Length);
                streamFrame.SetData(streamData, fin);
            }
            catch (Exception)
            {
                // Something went wrong.  Requeue the transfer.
                this.Enqueue(nextTransfer);
                throw;
            }

            return regular;
        }
    }
}
