using System;

namespace QuicSharp
{
    using System.Diagnostics;

    public class Packet
    {
        public static byte[] CommonHeader(uint? version, bool publicReset, ulong? connectionId, byte[] sequence, int lowOrderBytesNumber, byte? fecGroupNumberOffset)
        {
            var header = new byte[1 + (connectionId.HasValue ? 8 : 0) + (version.HasValue ? 4 : 0) + 6 + 1 + (fecGroupNumberOffset.HasValue ? 1 : 0)];
            var next = 0;

            #region Public Flags

            // Public Reset
            if (publicReset)
                header[0] ^= 0x02;
            
            switch (lowOrderBytesNumber)
            {
                case 2:
                    header[0] ^= 0x10;
                    break;
                case 4:
                    header[0] ^= 0x20;
                    break;
                case 6:
                    header[0] ^= 0x30;
                    break;
            }
            next++;
            #endregion

            #region Connection ID

            if (connectionId.HasValue)
            {
                header[0] ^= 0x30;
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
            #endregion


            // Quic Version
            if (version.HasValue)
            {
                header[0] ^= 0x01;
                var versionBytes = BitConverter.GetBytes(version.Value);
                header[next] ^= versionBytes[0];
                header[next+1] ^= versionBytes[1];
                header[next+2] ^= versionBytes[2];
                header[next+3] ^= versionBytes[3];
                next += 4;
            }

            // Sequence Number
            Array.Copy(sequence, 0, header, next, 6);
            next += 6;

            // Skip the private flags
            next++;

            // FEC byte
            if (fecGroupNumberOffset.HasValue)
            {
                header[next-1] ^= 0x02;
                header[next] ^= fecGroupNumberOffset.Value;
            }

            // All QUIC packets on the wire begin with a common header sized between 2 and 21 bytes.
            Debug.Assert(header.Length >= 2 && header.Length <= 21);

            return header;
        }
    }
}
