using System;
using System.Text;

namespace ABSoftware.ServerFiles.PacketUtils.Packets
{
    public class MessagePacket : Packet
    {
        public string Text { get; private set; }

        public override void LoadPacket(byte[] packetData)
        {
            Text = Encoding.UTF8.GetString(packetData);
            base.LoadPacket(packetData);
        }
    }
}
