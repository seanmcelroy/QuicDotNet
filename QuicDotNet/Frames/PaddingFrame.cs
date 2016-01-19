namespace QuicDotNet.Frames
{
    using System.Linq;

    public class PaddingFrame : AbstractFrameBase
    {
        private readonly int _size;

        public PaddingFrame(int size)
        {
            this._size = size;
        }

        public override byte[] ToByteArray()
        {
            return Enumerable.Repeat((byte)0x00, this._size).ToArray();
        }
    }
}
