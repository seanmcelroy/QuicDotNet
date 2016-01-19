using System;
using System.Text;

namespace QuicDotNet.Messages
{
    public static class MessageTags
    {
        public static readonly uint PAD = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("PAD\0"), 0);
        public static readonly uint SNI = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("SNI\0"), 0);
        public static readonly uint STK = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("STK\0"), 0);
        public static readonly uint VER = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("VER\0"), 0);
        public static readonly uint CCS = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("CCS\0"), 0);
        public static readonly uint NONC = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("NONC"), 0);
        public static readonly uint MSPC = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("MSPC"), 0);
        public static readonly uint AEAD = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("AEAD"), 0);
        public static readonly uint UAID = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("UAID"), 0);
        public static readonly uint SCID = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("SCID"), 0);
        public static readonly uint TCID = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("TCID"), 0);
        public static readonly uint PDMD = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("PDMD"), 0);
        public static readonly uint SRBF = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("SRBF"), 0);
        public static readonly uint ICSL = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("ICSL"), 0);
        public static readonly uint PUBS = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("PUBS"), 0);
        public static readonly uint SCLS = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("SCLS"), 0);
        public static readonly uint KEXS = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("KEXS"), 0);
        public static readonly uint COPT = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("COPT"), 0);
        public static readonly uint CCRT = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("CCRT"), 0);
        public static readonly uint IRTT = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("IRTT"), 0);
        public static readonly uint CETV = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("CETV"), 0);
        public static readonly uint CFCW = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("CFCW"), 0);
        public static readonly uint SFCW = BitConverter.ToUInt32(Encoding.ASCII.GetBytes("SFCW"), 0);
    }
}
