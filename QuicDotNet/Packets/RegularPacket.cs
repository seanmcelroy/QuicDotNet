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
        public byte[] MessageAuthenticationHash { get; private set; }
        private int? _headerLength;
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
            return (uint)PublicHeader(QuicClient.QUIC_VERSION, this.ConnectionId, this._packetNumber).Length;
        }

        public byte[] PadAndNullEncrypt()
        {
            if (this._finalBytes != null)
                throw new InvalidOperationException("Packet is already finalized");

            var bytes = this.ToByteArray();
            var padAmount = MTU - bytes.Length;

            this.AddFrame(new PaddingFrame(padAmount));
            var paddedBytes = this.ToByteArray();

            var bytesToHash = new byte[paddedBytes.Length - 12];
            // ReSharper disable once PossibleInvalidOperationException
            Array.Copy(paddedBytes, 0, bytesToHash, 0, this._headerLength.Value);
            Array.Copy(paddedBytes, this._headerLength.Value + 12, bytesToHash, this._headerLength.Value, bytesToHash.Length - this._headerLength.Value);

            this.MessageAuthenticationHash = Fnv1A128Hash(bytesToHash);
            Array.Copy(this.MessageAuthenticationHash, 0, paddedBytes, this._headerLength.Value, this.MessageAuthenticationHash.Length);

            this._finalBytes = paddedBytes;
            return this._finalBytes;
        }

        public override byte[] ToByteArray()
        {
            if (this._finalBytes != null)
                return this._finalBytes;

            var header = PublicHeader(QuicClient.QUIC_VERSION, this.ConnectionId, this._packetNumber);
            this._headerLength = header.Length;

            for (var i = 0; i < this._frames.Count; i++)
            {
                var key = this._frames.Keys.ElementAt(i);
                this._frames[key] = this._frames[key] ?? key.ToByteArray();
            }

            var frameByteArrays = this._frames.Select(f => f.Value).ToArray();
            var frameByteCount = frameByteArrays.Sum(f => f.Length);

            var bytes = new byte[header.Length + 12 + (this._fecGroup == null ? 1 : 2) + frameByteCount];

            // Apply public header
            Array.Copy(header, 0, bytes, 0, header.Length);
            var next = header.Length;

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
