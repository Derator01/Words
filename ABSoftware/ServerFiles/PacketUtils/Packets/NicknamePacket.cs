using System;
using System.Text;

namespace ABSoftware.ServerFiles.PacketUtils.Packets
{
    public class NicknamePacket : Packet
    {
        public string Nickname { get; private set; }

        public override void LoadPacket(byte[] packetData)
        {
            Nickname = Encoding.UTF8.GetString(packetData);
            base.LoadPacket(packetData);
        }
    }
}
