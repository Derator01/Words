using System;
using ABSoftware.ServerFiles.PacketUtils;
using ABSoftware.ServerFiles.PacketUtils.Packets;
using ABSoftware.ServerFiles.Users;

namespace ABSoftware.ServerFiles.Games
{
    interface INetListener
    {
        void IncomingPacket(AuthorizedUser user, Packet packet);
    }
}
