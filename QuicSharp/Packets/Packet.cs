using System;

namespace QuicDotNet
{
    using System.Diagnostics;
    using System.Linq;

    public class Packet
    {
        public const byte PUBLIC_FLAG_VERSION = 0x01;
        public const byte PUBLIC_FLAG_RESET = 0x02;
        
        public const byte PRIVATE_FLAG_ENTROPY = 0x01;
        public const byte PRIVATE_FLAG_FEC_GROUP = 0x02;
        public const byte PRIVATE_FLAG_FEC = 0x04;

        public const int MTU = 1370;

        private static byte[] PublicHeader(uint? version, ulong? connectionId, ulong packetNumber)
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
                header[0] ^= PUBLIC_FLAG_VERSION;
                var versionBytes = BitConverter.GetBytes(version.Value);
                header[next] ^= versionBytes[0];
                header[next+1] ^= versionBytes[1];
                header[next+2] ^= versionBytes[2];
                header[next+3] ^= versionBytes[3];
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

        public static byte[] VersionNegotiation(ulong connectionId, uint[] versionsSupported)
        {
            var packet = new byte[9 + versionsSupported.Length * 4];

            packet[0] |= PUBLIC_FLAG_VERSION;

            var cidBytes = BitConverter.GetBytes(connectionId);
            Array.Copy(cidBytes, 0, packet, 1, 8);

            var next = 9;
            foreach (var version in versionsSupported)
            {
                Array.Copy(BitConverter.GetBytes(version), 0, packet, next, 4);
                next += 4;
            }
            
            return packet;
        }

        internal static byte[] Regular(ulong connectionId, ulong packetNumber, byte? fecGroup)
        {
            var header = PublicHeader(QuicClient.QUIC_VERSION, connectionId, packetNumber);

            // Apply private header
            var bytes = fecGroup == null ? new byte[header.Length + 1] : new byte[header.Length + 2];

            Array.Copy(header, 0, bytes, 0, header.Length);
            bytes[header.Length] ^= PRIVATE_FLAG_FEC_GROUP;

            if (fecGroup != null)
                bytes[header.Length + 1] = fecGroup.Value;

            return bytes;
        }

        public static byte[] Frame(ulong connectionid, ulong packetNumber, byte? fecGroup, byte[][] frames)
        {
            var framesLength = frames.Sum(f => f.Length);

            var regular = Regular(connectionid, packetNumber, fecGroup);

            var bytes = new byte[regular.Length + framesLength];

            Array.Copy(regular, 0, bytes, 0, regular.Length);
            var i = regular.Length;
            foreach (var frame in frames)
            {
                Array.Copy(frame, 0, bytes, i, frame.Length);
                i += frame.Length;
            }

            return bytes;
        }
    }
}
