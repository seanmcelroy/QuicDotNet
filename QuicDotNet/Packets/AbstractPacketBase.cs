namespace QuicDotNet.Packets
{
    using System;
    using System.Diagnostics;

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

        protected static byte[] PublicHeader(uint? version, ulong? connectionId, ulong packetNumber)
        {
            var header = new byte[1 + (connectionId.HasValue ? 8 : 0) + (version.HasValue ? 4 : 0) + 6];

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
            if (version.HasValue)
            {
                header[0] ^= 0x01;
                var versionBytes = BitConverter.GetBytes(version.Value);
                header[next] ^= versionBytes[0];
                header[next + 1] ^= versionBytes[1];
                header[next + 2] ^= versionBytes[2];
                header[next + 3] ^= versionBytes[3];
                next += 4;
            }

            // Packet number
            {
                int pnLength;
                if (packetNumber <= byte.MaxValue)
                {
                    /* No-op, no need to apply 0x00 */
                    pnLength = 1;
                }
                else if (packetNumber <= ushort.MaxValue)
                {
                    header[0] ^= 0x10;
                    pnLength = 2;
                }
                else if (packetNumber <= uint.MaxValue)
                {
                    header[0] ^= 0x20;
                    pnLength = 4;
                }
                else
                {
                    header[0] ^= 0x30;
                    pnLength = 6;
                }

                var pnBytes = BitConverter.GetBytes(packetNumber);
                Array.Copy(pnBytes, Math.Max(0, pnBytes.Length - 6), header, next, pnLength);
            }


            // All QUIC packets on the wire begin with a common header sized between 2 and 19 bytes.
            Debug.Assert(header.Length >= 2 && header.Length <= 19);

            return header;
        }

        public abstract byte[] ToByteArray();
    }
}
