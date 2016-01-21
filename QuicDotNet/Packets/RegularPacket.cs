using System;

namespace QuicDotNet.Packets
{
    using System.Collections.Generic;
    using System.Linq;

    using JetBrains.Annotations;

    using QuicDotNet.Frames;

    public class RegularPacket : AbstractPacketBase
    {
        public const byte PRIVATE_FLAG_ENTROPY = 0x01;
        public const byte PRIVATE_FLAG_FEC_GROUP = 0x02;
        public const byte PRIVATE_FLAG_FEC = 0x04;

        private readonly ulong _packetNumber;
        private readonly byte? _fecGroup;
        private readonly Dictionary<AbstractFrameBase, byte[]> _frames = new Dictionary<AbstractFrameBase, byte[]>(5);

        [CanBeNull]
        public byte[] MessageAuthenticationHash { get; private set; }
        [CanBeNull]
        private byte[] HeaderBytes { get; set; }

        [CanBeNull]
        private byte[] _finalBytes;

        public RegularPacket(ulong connectionId, ulong packetNumber, byte? fecGroup) : base(connectionId)
        {
            this._packetNumber = packetNumber;
            this._fecGroup = fecGroup;
        }

        public void AddFrame([NotNull] AbstractFrameBase frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));
            if (this._finalBytes != null)
                throw new InvalidOperationException("Packet is already finalized");

            this._frames.Add(frame, null);
        }

        public uint GetHeaderLength()
        {
            if (this.HeaderBytes == null)
                this.HeaderBytes = PublicHeader(QuicClient.QUIC_VERSION, this.ConnectionId, this._packetNumber);

            return (uint)this.HeaderBytes.Length;
        }

        public byte[] PadAndNullEncrypt()
        {
            if (this._finalBytes != null)
                throw new InvalidOperationException("Packet is already finalized");
            if (this.HeaderBytes == null)
                this.HeaderBytes = PublicHeader(QuicClient.QUIC_VERSION, this.ConnectionId, this._packetNumber);

            var bytes = this.ToByteArray();
            var padAmount = MTU - bytes.Length;
            if (padAmount < 0)
                throw new InvalidOperationException($"Packet is too large at {bytes.Length} bytes with the MTU limit of {MTU}");

            if (padAmount > 0)
                this.AddFrame(new PaddingFrame(padAmount));

            var paddedBytes = this.ToByteArray();

            var bytesToHash = new byte[paddedBytes.Length - 12];
            // ReSharper disable once PossibleInvalidOperationException
            Array.Copy(paddedBytes, 0, bytesToHash, 0, this.HeaderBytes.Length);
            Array.Copy(paddedBytes, this.HeaderBytes.Length + 12, bytesToHash, this.HeaderBytes.Length, bytesToHash.Length - this.HeaderBytes.Length);

            this.MessageAuthenticationHash = Fnv1A128Hash(bytesToHash);
            Array.Copy(this.MessageAuthenticationHash, 0, paddedBytes, this.HeaderBytes.Length, this.MessageAuthenticationHash.Length);

            this._finalBytes = paddedBytes;
            return this._finalBytes;
        }

        public override byte[] ToByteArray()
        {
            if (this._finalBytes != null)
                return this._finalBytes;
            if (this.HeaderBytes == null)
                this.HeaderBytes = PublicHeader(QuicClient.QUIC_VERSION, this.ConnectionId, this._packetNumber);

            for (var i = 0; i < this._frames.Count; i++)
            {
                var key = this._frames.Keys.ElementAt(i);
                this._frames[key] = this._frames[key] ?? key.ToByteArray();
            }

            var frameByteArrays = this._frames.Select(f => f.Value).ToArray();
            var frameByteCount = frameByteArrays.Sum(f => f.Length);

            var bytes = new byte[this.HeaderBytes.Length + 12 + (this._fecGroup == null ? 1 : 2) + frameByteCount];

            // Apply public header
            Array.Copy(this.HeaderBytes, 0, bytes, 0, this.HeaderBytes.Length);
            var next = this.HeaderBytes.Length;

            // Apply message authentication hash (only for null-encrypted)
            if (this.MessageAuthenticationHash != null)
                Array.Copy(this.MessageAuthenticationHash, 0, bytes, next, 12);
            next += 12;

            // Apply private header
            if (this._fecGroup == null)
                next++;
            else
            {
                bytes[next] ^= PRIVATE_FLAG_FEC_GROUP;
                bytes[next + 1] = this._fecGroup.Value;
                next += 2;
            }

            // Add frames
            foreach (var fba in frameByteArrays)
            {
                Array.Copy(fba, 0, bytes, next, fba.Length);
                next += fba.Length;
            }

            return bytes;
        }
    }
}
