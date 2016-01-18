using System;

namespace QuicDotNet.Frames
{
    using System.Diagnostics;

    using JetBrains.Annotations;

    public class StreamFrame : AbstractFrameBase
    {
        /// <summary>
        /// Creates a PROTOTYPE stream frame with a block of data to send for the stream
        /// A prototype frame can only service the <see cref="StreamFrame.GetMetadataLength"/> method until it is hydrated with the <see cref="StreamFrame.SetData(byte[], bool)" method/>
        /// </summary>
        /// <param name="streamId">The StreamId for the frame</param>
        /// <param name="offset">A variable-sized unsigned number specifying the byte offset in the actual larger stream for this block of data.</param>
        public StreamFrame(uint streamId, ulong offset)
        {
            this._streamId = streamId;
            this._offset = offset;

            // A stream frame must always have either non-zero data length or the FIN bit set.
            Debug.Assert(this.Fin || this._data.Length > 0);
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

        private readonly uint _streamId;

        private readonly ulong _offset;

        private uint? _metadataLength;

        private byte? _slen;

        private byte? _olen;

        public override uint GetMetadataLength()
        {
            if (this._streamId <= byte.MaxValue)
                this._slen = 0x00;
            else if (this._streamId <= ushort.MaxValue)
                this._slen = 0x01;
            else if (this._streamId <= 16777215)
                this._slen = 0x02;
            else
                this._slen = 0x03;

            if (this._offset <= byte.MaxValue)
                this._olen = 0x00;
            else if (this._offset <= ushort.MaxValue)
                this._olen = 0x01;
            else if (this._offset <= 16777215)
                this._olen = 0x02;
            else if (this._offset <= 4294967295)
                this._olen = 0x03;
            else if (this._offset <= 1099511627775)
                this._olen = 0x04;
            else if (this._offset <= 281474976710655)
                this._olen = 0x05;
            else if (this._offset <= 72057594037927935)
                this._olen = 0x06;
            else
                this._olen = 0x07;

            // INFO: We're going to just always send data length (+2)
            this._metadataLength = 1U + this._slen.Value + this._olen.Value + 2U;
            return this._metadataLength.Value;
        }

        public void SetData([NotNull] byte[] data, bool fin)
        {
            if (this._data != null)
                throw new InvalidOperationException("Stream frame is not prototyped and is already hydrated with binary data");

            this._data = data;
            this.Fin = fin;

            Debug.Assert(this.Fin || this._data.Length > 0);
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
            Array.Copy(streamIdBytes, 0, bytes, 1, slen);

            /* Offset: A variable-sized unsigned number specifying the byte
             * offset in the stream for this block of data.
             */
            var offsetBytes = BitConverter.GetBytes(this._offset);
            Array.Copy(offsetBytes, 0, bytes, slen + 1, olen);

            /* Data length: An optional 16-bit unsigned number specifying the
             * length of the data in this stream frame.  The option to omit the
             * length should only be used when the packet is a "full-sized"
             * Packet, to avoid the risk of corruption via padding.
             */
            Array.Copy(BitConverter.GetBytes(this._data.Length), 0, bytes, slen + olen + 1, 2);

            // Stream data
            Array.Copy(this._data, 0, bytes, slen + olen + 3, this._data.Length);

            return bytes;
        }
    }
}
