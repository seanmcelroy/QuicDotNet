namespace QuicDotNet
{
    using System;
    using System.Diagnostics;

    using QuicDotNet.Frames;

    public class QuicClient : UdpClient
    {
        public const uint QUIC_VERSION = 81485053;

        private bool versionAgreed;

        public void Connect()
        {
            var random = new Random(Environment.TickCount);

            var cid = Convert.ToUInt64(random.Next(1000, int.MaxValue));

            var packetNumber = new byte[6];
            random.NextBytes(packetNumber);

            var streamFrame = Frame.Stream(false, false, 1, 0);


            var commonHeader = Packet.Frame(cid, 1, null, null);

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
