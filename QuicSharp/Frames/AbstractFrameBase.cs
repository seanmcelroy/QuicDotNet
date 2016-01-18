namespace QuicDotNet.Frames
{
    public abstract class AbstractFrameBase
    {
        public enum FrameType : byte
        {
            STREAM = 0x80,

            ACK = 0x40,

            CONGESTION_FEEDBACK = 0x20,

            PADDING = 0x00,

            RST_STREAM = 0x01,

            CONNECTION_CLOSE = 0x02,

            GOAWAY = 0x03,

            WINDOW_UPDATE = 0x04,

            BLOCKED = 0x05,

            STOP_WAITING = 0x06,

            PING = 0x07
        }

        public abstract uint GetMetadataLength();

        public abstract byte[] ToByteArray();
    }
}
