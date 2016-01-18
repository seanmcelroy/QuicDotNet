using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuicDotNet
{
    public class Message
    {
        private static byte[] Handshake(string tag, Dictionary<uint, byte[]> tagValuePairs)
        {
            var bytes = new byte[4 + 2 + 2 + (4+4) * tagValuePairs.Count + tagValuePairs.Sum(p => p.Value.Length)];
            
            // The tag of the message
            var tagBytes = Encoding.UTF8.GetBytes(tag);
            Array.Copy(tagBytes, 0, bytes, 0, 4);

            // A uint16 containing the number of tag - value pairs.
            var pairCountBytes = BitConverter.GetBytes(Convert.ToUInt16(tagValuePairs));
            Array.Copy(pairCountBytes, 0, bytes, 4, 2);

            // Two bytes of padding which should be zero when sent but ignored when received.
            var next = 8;

            uint endOffset = 8;
            foreach (var tvp in tagValuePairs)
            {
                tagBytes = BitConverter.GetBytes(tvp.Key);
                Array.Copy(tagBytes, 0, bytes, next, 4);
                next += 4;

                endOffset += (uint)tvp.Value.Length;
                var endOffsetBytes = BitConverter.GetBytes(endOffset);
                Array.Copy(endOffsetBytes, 0, bytes, next, 4);
                next += 4;
            }

            foreach (var tvp in tagValuePairs)
            {
                Array.Copy(tvp.Value, 0, bytes, next, tvp.Value.Length);
                next += tvp.Value.Length;
            }

            return bytes;
        }
    }
}
