namespace QuicDotNet.Packets
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Numerics;
    using System.Text;

    using JetBrains.Annotations;

    public abstract class AbstractPacketBase
    {
        public const ushort MTU = 1370;

        public enum PacketType : byte
        {
            VERSION_NEGOTIATION = 0x01,
            PUBLIC_RESET = 0x02,
            REGULAR = 0x04
        }

        protected AbstractPacketBase(ulong connectionId)
        {
            this.ConnectionId = connectionId;
        }

        protected ulong ConnectionId { get; private set; }

        public static byte[] Fnv1A128Hash(byte[] bytes)
        {
            var modValue = BigInteger.Parse("100000000000000000000000000000000", NumberStyles.AllowHexSpecifier);
            var fnvPrime = BigInteger.Parse("0000000001000000000000000000013B", NumberStyles.AllowHexSpecifier);
            var fnvOffsetBasis = BigInteger.Parse("6C62272E07BB014262B821756295C58D", NumberStyles.AllowHexSpecifier);

            var hash = fnvOffsetBasis;

            foreach (var t in bytes)
            {
                unchecked
                {
                    hash ^= t;
                    hash = hash * fnvPrime % modValue;
                }
            }

            // QUIC changed this from 16 to 12.
            return hash.ToByteArray().Take(12).ToArray();
        }

        protected static byte[] PublicHeader([CanBeNull] string version, ulong? connectionId, ulong packetNumber)
        {
            // Packet number
            int pnLength;
            {
                if (packetNumber <= byte.MaxValue)
                    /* No-op, no need to apply 0x00 */
                    pnLength = 1;
                else if (packetNumber <= ushort.MaxValue)
                    pnLength = 2;
                else if (packetNumber <= uint.MaxValue)
                    pnLength = 4;
                else
                    pnLength = 6;
            }

            var header = new byte[1 + (connectionId.HasValue ? 8 : 0) + (!string.IsNullOrWhiteSpace(version) ? 4 : 0) + pnLength];

            // Public Flags
            var next = 1;

            // Connection ID
            if (connectionId.HasValue)
            {
                header[0] |= 0x0C;
                var cidBytes = BitConverter.GetBytes(connectionId.Value);
                header[next] ^= cidBytes[0];
                header[next + 1] ^= cidBytes[1];
                header[next + 2] ^= cidBytes[2];
                header[next + 3] ^= cidBytes[3];
                header[next + 4] ^= cidBytes[4];
                header[next + 5] ^= cidBytes[5];
                header[next + 6] ^= cidBytes[6];
                header[next + 7] ^= cidBytes[7];
                next += 8;
            }

            // Quic Version
            if (!string.IsNullOrWhiteSpace(version))
            {
                header[0] ^= 0x01;
                var versionBytes = Encoding.ASCII.GetBytes(version);
                header[next] ^= versionBytes[0];
                header[next + 1] ^= versionBytes[1];
                header[next + 2] ^= versionBytes[2];
                header[next + 3] ^= versionBytes[3];
                next += 4;
            }

            // Packet number
            {
                if (packetNumber <= byte.MaxValue)
                {
                    /* No-op, no need to apply 0x00 */
                }
                else if (packetNumber <= ushort.MaxValue)
                    header[0] ^= 0x10;
                else if (packetNumber <= uint.MaxValue)
                    header[0] ^= 0x20;
                else
                    header[0] ^= 0x30;

                var pnBytes = BitConverter.GetBytes(packetNumber);
                Array.Copy(pnBytes, 0, header, next, pnLength);
            }
            
            // All QUIC packets on the wire begin with a common header sized between 2 and 19 bytes.
            Debug.Assert(header.Length >= 2 && header.Length <= 19);

            return header;
        }

        public abstract byte[] ToByteArray();
    }
}
