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
        public const ushort MTU = 1350;

        public enum PublicFlags : byte
        {
            /// <summary>
            /// Interpretation of this flag
            /// depends on whether the packet is sent by the server or the
            /// client.  When sent by the client, setting it indicates that the
            /// header contains a QUIC Version (see below).  This bit must be
            /// set by a client in all packets until confirmation from the
            /// server arrives agreeing to the proposed version is received by
            /// the client.  A server indicates agreement on a version by
            /// sending packets without setting this bit.  When this bit is set
            /// by the server, the packet is a Version Negotiation Packet.
            /// Version Negotiation is described in more detail later.
            /// <seealso cref="https://tools.ietf.org/html/draft-tsvwg-quic-protocol-02#section-6.1"/>
            /// </summary>
            PUBLIC_FLAG_VERSION = 0x01,
            /// <summary>
            /// Set to indicate that the packet is a Public Reset packet.
            /// </summary>
            PUBLIC_FLAG_RESET = 0x02
        }

        protected AbstractPacketBase(ulong? connectionId)
        {
            this.ConnectionId = connectionId;
        }

        public ulong? ConnectionId { get; private set; }

        [NotNull, Pure]
        public static byte[] Fnv1A128Hash([NotNull] byte[] bytes)
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

        [Pure, NotNull]
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

        public static bool TryParse(byte[] packetBytes, out AbstractPacketBase packet)
        {
            var publicFlags = packetBytes[0];
            var index = 1;
            var versionFlag = (publicFlags & (1 << 0)) != 0;
            var resetFlag = (publicFlags & (1 << 1)) != 0;
            var cidFlag1 = (publicFlags & (1 << 2)) != 0;
            var cidFlag2 = (publicFlags & (1 << 3)) != 0;
            var pnFlag1 = (publicFlags & (1 << 4)) != 0;
            var pnFlag2 = (publicFlags & (1 << 5)) != 0;

            ulong? connectionId;
            if (cidFlag1 && cidFlag2)
            {
                connectionId = BitConverter.ToUInt64(packetBytes, index);
                index += 8;
            }
            else if (!cidFlag1 && cidFlag2)
            {
                connectionId = BitConverter.ToUInt32(packetBytes, index);
                index += 4;
            }
            else if (cidFlag1)
            {
                connectionId = packetBytes[1];
                index += 1;
            }
            else
                connectionId = null;

            uint? version;
            if (versionFlag)
            {
                version = BitConverter.ToUInt32(packetBytes, index);
                index += 4;
            }

            ulong packetNumber;
            if (pnFlag1 && pnFlag2)
            {
                var ba = new byte[8];
                Array.Copy(packetBytes, index, ba, 2, 6);
                packetNumber = BitConverter.ToUInt64(ba, 0);
                index += 6;
            }
            else if (!pnFlag1 && pnFlag2)
            {
                packetNumber = BitConverter.ToUInt32(packetBytes, index);
                index += 4;
            }
            else if (pnFlag1)
            {
                packetNumber = BitConverter.ToUInt16(packetBytes, index);
                index += 2;
            }
            else
            {
                packetNumber = packetBytes[index];
                index += 1;
            }

            var rp = new RegularPacket(connectionId, packetNumber, null);
            var payloadBytes = new byte[packetBytes.Length - index];
            Array.Copy(packetBytes, index, payloadBytes, 0, payloadBytes.Length);
            rp.FromByteArray(payloadBytes);
            packet = rp;

            return true;
        }
    }
}
