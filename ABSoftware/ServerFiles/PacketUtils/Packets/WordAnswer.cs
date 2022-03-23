using System;
using System.Text;

namespace ABSoftware.ServerFiles.PacketUtils.Packets
{
    public class WordAnswer : Packet
    {
        public string Word;

        public override void LoadPacket(byte[] packetData)
        {
            this.Word = Encoding.UTF8.GetString(packetData);
            base.LoadPacket(packetData);
        }
    }
}
