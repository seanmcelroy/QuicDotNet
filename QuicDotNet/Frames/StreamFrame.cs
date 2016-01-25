using System;

namespace QuicDotNet.Frames
{
    using System.Diagnostics;

    using JetBrains.Annotations;

    public class StreamFrame : AbstractFrameBase
    {
        private StreamFrame()
        {
        }

        /// <summary>
        /// Creates a PROTOTYPE stream frame with a block of data to send for the stream
        /// A prototype frame can only service the <see cref="StreamFrame.GetMetadataLength"/> method until it is hydrated with the <see cref="StreamFrame.SetData(byte[], bool)"/> method
        /// </summary>
        /// <param name="streamId">The StreamId for the frame</param>
        /// <param name="offset">A variable-sized unsigned number specifying the byte offset in the actual larger stream for this block of data.</param>
        public StreamFrame(uint streamId, ulong offset)
        {
            this._streamId = streamId;
            this._offset = offset;
        }
        
        /// <summary>
        /// Creates a new stream frame with a block of data to send for the stream
        /// </summary>
        /// <param name="data">The data block to send in this stream frame</param>
        /// <param name="fin">A value indicating whether this is the final block of data for this stream, and that it should enter a half-closed state</param>
        /// <param name="streamId">The StreamId for the frame</param>
        /// <param name="offset">A variable-sized unsigned number specifying the byte offset in the actual larger stream for this block of data.</param>
        public StreamFrame([NotNull] byte[] data, bool fin, uint streamId, ulong offset)
        {
            this._data = data;
            this.Fin = fin;
            this._streamId = streamId;
            this._offset = offset;

            // A stream frame must always have either non-zero data length or the FIN bit set.
            Debug.Assert(this.Fin || this._data.Length > 0);
        }

        private byte[] _data;

        private bool Fin { get; set; }

        private uint _streamId;

        private readonly ulong _offset;

        private int? _metadataLength;

        private byte? _slen;

        private byte? _olen;

        public int GetMetadataLength()
        {
            if (this._streamId <= byte.MaxValue)
                this._slen = 0x00;
            else if (this._streamId <= ushort.MaxValue)
                this._slen = 0x01;
            else if (this._streamId <= 16777215)
                this._slen = 0x02;
            else
                this._slen = 0x03;

            if (this._offset == 0)
                this._olen = 0x00;
            else if (this._offset < 65536) // 16 bits
                this._olen = 0x01;
            else if (this._offset < 16777216) // 24 bits
                this._olen = 0x02;
            else if (this._offset < 4294967296) // 32 bits
                this._olen = 0x03;
            else if (this._offset < 1099511627776) // 40 bits
                this._olen = 0x04;
            else if (this._offset < 281474976710656) // 48 bits
                this._olen = 0x05;
            else if (this._offset < 72057594037927936) // 56 bits
                this._olen = 0x06;
            else // 64 bits
                this._olen = 0x07;

            // INFO: We're going to just always send data length (+2)
            this._metadataLength = 1 + (this._slen.Value + 1) * 8 + (this._olen.Value == 0 ? 0 : (this._olen.Value + 1) * 8) + 2;
            return this._metadataLength.Value;
        }

        public void SetData([NotNull] byte[] data, bool fin)
        {
            if (this._data != null)
                throw new InvalidOperationException("Stream frame is not prototyped and is already hydrated with binary data");

            this._data = data;
            this.Fin = fin;
            
            // A stream frame must always have either non-zero data length or the FIN bit set.
            Debug.Assert(this.Fin || this._data.Length > 0);

            // Clear pre-calculated metadata, as slen and olen could have changed.
            this._metadataLength = null;
            this._slen = null;
            this._olen = null;
        }

        public static Tuple<StreamFrame, int> FromByteArray(byte[] bytes, int index)
        {
            var finFlag = (bytes[index] & (1 << 6)) != 0;
            var dataLengthFlag = (bytes[index] & (1 << 5)) != 0;
            var olenIndex = (bytes[index] >> 2) & 7;

            var frame = new StreamFrame
                        {
                            Fin = finFlag,
                            _olen = (byte)(olenIndex == 0 ? 0 : olenIndex + 1),
                            _slen = (byte)(bytes[index] & 3)
                        };

            switch (frame._slen)
            {
                case 0:
                    frame._streamId = bytes[index + 1];
                    break;
                case 1:
                    frame._streamId = BitConverter.ToUInt16(bytes, index + 1);
                    break;
                case 2:
                    var buf = new byte[4];
                    Array.Copy(bytes, index + 1, buf, 1, 3);
                    frame._streamId = BitConverter.ToUInt32(buf,0);
                    break;
                case 3:
                    frame._streamId = BitConverter.ToUInt32(bytes, index + 1);
                    break;
            }

            // ReSharper disable PossibleInvalidOperationException
            frame._metadataLength = 1 + (frame._slen.Value + 1) + (frame._olen.Value == 0 ? 0 : frame._olen.Value + 1) + (dataLengthFlag ? 2 : 0);
            // ReSharper restore PossibleInvalidOperationException

            index += frame._metadataLength.Value;
            var dataLength = dataLengthFlag ? BitConverter.ToUInt16(bytes, index - 2) : bytes.Length - index - 1;
            frame._data = new byte[dataLength];
            Array.Copy(bytes, index, frame._data, 0, dataLength);
            index += dataLength;

            return new Tuple<StreamFrame, int>(frame, index);
        }

        public override byte[] ToByteArray()
        {
            if (this._data == null)
                throw new InvalidOperationException("Stream frame is prototyped but not hydrated with binary data");

            if (this._metadataLength == null)
                this.GetMetadataLength();

            // ReSharper disable PossibleInvalidOperationException
            var metadataLength = this._metadataLength.Value;
            var slen = this._slen.Value;
            var olen = this._olen.Value;
            // ReSharper restore PossibleInvalidOperationException
            
            // INFO: We're going to just always send data length (+2)
            var bytes = new byte[metadataLength + this._data.Length];

            bytes[0] = (byte)FrameType.STREAM;
            if (this.Fin)
                bytes[0] |= 0x40;

            // INFO: We're going to just always send data length
            bytes[0] |= 0x20;

            bytes[0] |= (byte)(olen << 2);
            bytes[0] |= slen;

            /* Stream ID: A variable-sized unsigned ID unique to this stream.
             */
            var streamIdBytes = BitConverter.GetBytes(this._streamId);
            Array.Copy(streamIdBytes, 0, bytes, 1, slen + 1);

            /* Offset: A variable-sized unsigned number specifying the byte
             * offset in the stream for this block of data.
             */
            var next = slen + 2;
            if (olen > 0)
            {
                var offsetBytes = BitConverter.GetBytes(this._offset);
                Array.Copy(offsetBytes, 0, bytes, next, olen == 0 ? 0 : olen + 1);
                next += olen == 0 ? 0 : olen + 1;
            }

            /* Data length: An optional 16-bit unsigned number specifying the
             * length of the data in this stream frame.  The option to omit the
             * length should only be used when the packet is a "full-sized"
             * Packet, to avoid the risk of corruption via padding.
             */
            Array.Copy(BitConverter.GetBytes(this._data.Length), 0, bytes, next, 2);
            next += 2;

            // Stream data
            Array.Copy(this._data, 0, bytes, next, this._data.Length);

            return bytes;
        }
    }
}
