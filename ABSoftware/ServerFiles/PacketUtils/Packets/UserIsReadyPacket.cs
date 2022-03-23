using System;

namespace ABSoftware.ServerFiles.PacketUtils.Packets
{
    public class UserIsReadyPacket : Packet
    {
        public bool isReady { get; set; }

        public override void LoadPacket(byte[] packetData)
        {
            isReady = packetData[0].Equals(0x01) ? true : false;
            base.LoadPacket(packetData);
        }
    }
}
