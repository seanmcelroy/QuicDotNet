using System;

namespace QuicDotNet.Packets
{
    using System.Collections.Generic;
    using System.Diagnostics;
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
        private readonly Dictionary<Frames.AbstractFrameBase, byte[]> _frames = new Dictionary<Frames.AbstractFrameBase, byte[]>(5);
        private byte[] messageAuthenticationHash = null;

        public RegularPacket(ulong connectionId, ulong packetNumber, byte? fecGroup) : base(connectionId)
        {
            this._packetNumber = packetNumber;
            this._fecGroup = fecGroup;
        }

        public void AddFrame([NotNull] AbstractFrameBase frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            this._frames.Add(frame, null);
        }

        public uint GetHeaderLength()
        {
            return (uint)PublicHeader(QuicClient.QUIC_VERSION, this.ConnectionId, this._packetNumber).Length;
        }

        public void PadAndNullEncrypt()
        {
            var bytes = this.ToByteArray();
            var padAmount = MTU - bytes.Length - 12;

            this.AddFrame(new PaddingFrame(padAmount));
            var paddedBytes = this.ToByteArray();
            Debug.WriteLine(paddedBytes.GenerateHexDumpWithASCII());

            this.messageAuthenticationHash = Fnv1A128Hash(paddedBytes);

            Debug.WriteLine("Message authentication hash: " + this.messageAuthenticationHash.Select(b => b.ToString("x2")).Aggregate((c,n)=>c+" "+n));
            Debug.WriteLine(this.ToByteArray().GenerateHexDumpWithASCII());
        }

        public override byte[] ToByteArray()
        {
            var header = PublicHeader(QuicClient.QUIC_VERSION, this.ConnectionId, this._packetNumber);

            for (var i = 0; i < this._frames.Count; i++)
            {
                var key = this._frames.Keys.ElementAt(i);
                this._frames[key] = this._frames[key] ?? key.ToByteArray();
            }

            var frameByteArrays = this._frames.Select(f => f.Value).ToArray();
            var frameByteCount = frameByteArrays.Sum(f => f.Length);

            var bytes = new byte[header.Length + (this.messageAuthenticationHash == null ? 0 : 12) + (this._fecGroup == null ? 1 : 2) + frameByteCount];

            // Apply public header
            Array.Copy(header, 0, bytes, 0, header.Length);
            var next = header.Length;

            // Apply message authentication hash (only for null-encrypted)
            if (this.messageAuthenticationHash != null)
            {
                Array.Copy(this.messageAuthenticationHash, 0, bytes, next, 12);
                next += 12;
            }

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
