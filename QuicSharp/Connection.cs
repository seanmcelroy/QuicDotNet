namespace QuicSharp
{
    using System;
    using System.Diagnostics;

    public class Connection
    {
        public const uint QUIC_VERSION = 4;

        private bool versionAgreed;

        public void Connect()
        {
            var random = new Random(Environment.TickCount);

            var sequence = new byte[6];
            random.NextBytes(sequence);

            var commonHeader = Packet.CommonHeader(QUIC_VERSION, false, ulong.MaxValue - 300, sequence, 0, null);

            /*
                Bit at 0x01 is set to indicate that the packet contains a QUIC
                Version.  This bit must be set by a client in all packets until
                confirmation from the server arrives agreeing to the proposed
                version is received by the client.
             */
            if (!this.versionAgreed)
                commonHeader[0] |= 0x01;

            Debug.WriteLine(commonHeader);
        }
    }
}
