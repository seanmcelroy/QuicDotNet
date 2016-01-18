using System;

namespace QuicDotNet.Packets
{
    public class VersionNegotiationPacket : AbstractPacketBase
    {
        private readonly uint[] _versionsSupported;

        public VersionNegotiationPacket(ulong connectionId, uint[] versionsSupported) : base(connectionId)
        {
            this._versionsSupported = versionsSupported;
        }

        public override byte[] ToByteArray()
        {
            var packet = new byte[9 + this._versionsSupported.Length * 4];

            packet[0] |= 0x01;

            var cidBytes = BitConverter.GetBytes(this.ConnectionId);
            Array.Copy(cidBytes, 0, packet, 1, 8);

            var next = 9;
            foreach (var version in this._versionsSupported)
            {
                Array.Copy(BitConverter.GetBytes(version), 0, packet, next, 4);
                next += 4;
            }

            return packet;
        }
    }
}
