namespace QuicDotNet.Test.Unit.Packets
{
    using System.Linq;
    using System.Text;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using QuicDotNet.Frames;
    using QuicDotNet.Messages;
    using QuicDotNet.Packets;

    [TestClass]
    public class PacketTest
    {
        [TestMethod]
        public void FreshHello()
        {
            var connectionId = 15690248817103694251U;

            var packet = new RegularPacket(connectionId, 1, null);
            packet.AddFrame(new StreamFrame(new ClientHandshakeMessage
                                            {
                                                { MessageTags.PAD, Enumerable.Repeat((byte)0x2d, 1053).ToArray() },
                                                { MessageTags.SNI, "clients2.google.com" },
                                                { MessageTags.VER, "Q025" },
                                                { MessageTags.CCS, new byte[] { 0x7b, 0x26, 0xe9, 0xe7, 0xe4, 0x5c, 0x71, 0xff, 0x01, 0xe8, 0x81, 0x60, 0x92, 0x92, 0x1a, 0xe8 } },
                                                { MessageTags.MSPC, 100 },
                                                { MessageTags.UAID, "Chrome/49.0.2623.0 Windows NT 6.1; WOW64"},
                                                { MessageTags.TCID, 0},
                                                { MessageTags.PDMD, "X509"},
                                                { MessageTags.SRBF, 1048576},
                                                { MessageTags.ICSL, 30},
                                                { MessageTags.SCLS, 1},
                                                { MessageTags.COPT, 1146636614},
                                                { MessageTags.IRTT, 9248},
                                                { MessageTags.CFCW, 15728640},
                                                { MessageTags.SFCW, 6291456}
                                            }, false, 1, 0));
            packet.PadAndNullEncrypt();



        }
    }
}
