using System;

namespace QuicDotNet.Packets
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using JetBrains.Annotations;

    public class RegularPacket : AbstractPacketBase
    {
        public const byte PRIVATE_FLAG_ENTROPY = 0x01;
        public const byte PRIVATE_FLAG_FEC_GROUP = 0x02;
        public const byte PRIVATE_FLAG_FEC = 0x04;

        private readonly ulong _packetNumber;
        private readonly byte? _fecGroup;
        private readonly List<Frames.AbstractFrameBase> _frames = new List<Frames.AbstractFrameBase>(5);

        public RegularPacket(ulong connectionId, ulong packetNumber, byte? fecGroup) : base(connectionId)
        {
            this._packetNumber = packetNumber;
            this._fecGroup = fecGroup;
        }

        public void AddFrame([NotNull] Frames.AbstractFrameBase frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            this._frames.Add(frame);
        }

        public uint GetHeaderLength()
        {
            return (uint)PublicHeader(QuicClient.QUIC_VERSION, this.ConnectionId, this._packetNumber).Length;
        }

        public override byte[] ToByteArray()
        {
            var header = PublicHeader(QuicClient.QUIC_VERSION, this.ConnectionId, this._packetNumber);

            var frameByteArrays = this._frames.Select(f => f.ToByteArray()).ToList();
            var frameByteCount = frameByteArrays.Sum(f => f.Length);

            var bytes = this._fecGroup == null ? new byte[header.Length + 1 + frameByteCount] : new byte[header.Length + 2 + frameByteCount];

            // Apply private header
            Array.Copy(header, 0, bytes, 0, header.Length);
            bytes[header.Length] ^= PRIVATE_FLAG_FEC_GROUP;

            if (this._fecGroup != null)
                bytes[header.Length + 1] = this._fecGroup.Value;

            // Add frames
            var next = this._fecGroup == null ? header.Length + 1 : header.Length + 2;

            foreach (var fba in frameByteArrays)
            {
                Array.Copy(fba, 0, bytes, next, fba.Length);
                next += fba.Length;
            }

            return bytes;
        }
    }
}
