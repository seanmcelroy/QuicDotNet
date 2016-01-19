using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuicDotNet.Messages
{
    public class ClientHandshakeMessage : Dictionary<uint, byte[]>
    {
        private static readonly byte[] _Chlo = Encoding.ASCII.GetBytes("CHLO");

        public ClientHandshakeMessage() : base(30)
        {
        }
        public ClientHandshakeMessage(IDictionary<uint, byte[]> tagValuePairs) : base(tagValuePairs)
        {
        }

        public static implicit operator byte[](ClientHandshakeMessage message)
        {
            return message.ToByteArray();
        }

        public void Add(uint key, string value)
        {
            base.Add(key, Encoding.ASCII.GetBytes(value));
        }

        public void Add(uint key, int value)
        {
            base.Add(key, BitConverter.GetBytes(Convert.ToUInt32(value)));
        }

        public byte[] ToByteArray()
        {
            var bytes = new byte[4 + 2 + 2 + (4 + 4) * this.Count + this.Sum(p => p.Value.Length)];

            // The tag of the message
            var tagBytes = _Chlo;
            Array.Copy(tagBytes, 0, bytes, 0, 4);

            // A uint16 containing the number of tag - value pairs.
            var pairCountBytes = BitConverter.GetBytes(Convert.ToUInt16(this.Count));
            Array.Copy(pairCountBytes, 0, bytes, 4, 2);

            // Two bytes of padding which should be zero when sent but ignored when received.
            var next = 8;

            uint endOffset = 0;
            foreach (var tvp in this)
            {
                tagBytes = BitConverter.GetBytes(tvp.Key);
                Array.Copy(tagBytes, 0, bytes, next, 4);
                next += 4;

                endOffset += (uint)tvp.Value.Length;
                var endOffsetBytes = BitConverter.GetBytes(endOffset);
                Array.Copy(endOffsetBytes, 0, bytes, next, 4);
                next += 4;
            }

            foreach (var tvp in this)
            {
                Array.Copy(tvp.Value, 0, bytes, next, tvp.Value.Length);
                next += tvp.Value.Length;
            }

            return bytes;
        }
    }
}
