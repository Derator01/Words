using System;
using System.Text;
using System.IO;
using ABSoftware;
using ABSoftware.ServerFiles;
using ABSoftware.ServerFiles.Utils;
using ABSoftware.ServerFiles.PacketUtils;
using ABSoftware.ServerFiles.PacketUtils.Packets;
using ABSoftware.ServerFiles.Users;
using ABSoftware.ServerFiles.Games;

namespace WordsGameServer
{
    class Program : Server
    {
        public static Program instance;

        static void Main(string[] args)
        {
            Config.LoadConfig();
            Program p = new Program();
            instance = p;
            p.Start(Config.Port);
        }

        ArrayList<AuthorizedUser> users = new ArrayList<AuthorizedUser>();

        #region Authorized User controls
        #region Has User
        public bool HasUser(Client socket)
        {
            for(int i = 0; i < users.Size; i++)
            {
                if (users[i].socket.ID.Equals(socket.ID))
                    return true;
            }
            return false;
        }

        public bool HasUser(string nickname)
        {
            for (int i = 0; i < users.Size; i++)
            {
                if (users[i].Nickname.Equals(nickname))
                    return true;
            }
            return false;
        }
        #endregion
        #region Add User
        public bool AddUser(Client socket, string nickname)
        {
            if (!HasUser(nickname))
                users.Add(new AuthorizedUser(socket, nickname));
            return false;
        }
        #endregion
        #region Get User
        public AuthorizedUser GetUser(Client socket)
        {
            for (int i = 0; i < users.Size; i++)
            {
                if (users[i].socket.ID.Equals(socket.ID))
                    return users[i];
            }
            return null;
        }

        public AuthorizedUser GetUser(string nickname)
        {
            for (int i = 0; i < users.Size; i++)
            {
                if (users[i].Nickname.Equals(nickname))
                    return users[i];
            }
            return null;
        }
        #endregion
        #region Remove User
        public bool RemoveUser(AuthorizedUser user)
        {
            if (user != null && users.Contains(user))
            {
                users.Remove(user);
                return true;
            }
            return false;
        }
        #endregion
        #endregion

        #region Game fields
        public RealWordsController realWordsController;
        #endregion

        public override void OnServerStart()
        {
            Config.LoadConfig();
            Println("The server is running on port " + Port, ConsoleColor.Cyan);
            realWordsController = new RealWordsController(this);
        }

        public override void OnServerStop()
        {
            Println("The server was successfully stopped!", ConsoleColor.Cyan);
            realWordsController = null;
        }

        public override void OnClientConnect(Client client)
        {
            Console.WriteLine($"Client {client.ID} has connected from {client.IP}!");
            client.Send(packetBuilder.Build((int)PacketUtils.PacketIds.Connected, new byte[] { }));
        }

        public override void OnClientDisconnect(Client client)
        {
            Console.WriteLine($"Client {client.ID} has disconnected from the server!");
            realWordsController.RemoveUser(realWordsController.GetUser(GetUser(client)));
            RemoveUser(GetUser(client));
        }

        public override void OnConsoleInput(string input)
        {
            switch(input.ToLower())
            {
                case "start":
                    if(!Start(Config.Port))
                    {
                        Console.WriteLine("The server is already running!");
                    }
                    break;
                case "stop":
                    if(!Stop())
                    {
                        Console.WriteLine("The server is already stopped!");
                    }
                    break;
                default:
                    Console.WriteLine("Command not found!");
                    break;
            }
        }

        public override void OnIncomingPacket(Client client, byte[] packet)
        {
            Packet disassembled = PacketDisassembler.Disassemble(packet);
            if (disassembled == null)
                return;
            Type packType = disassembled.GetType();
            if(packType.Equals(typeof(MessagePacket)))
            {
                if (!HasUser(client))
                    return;
                AuthorizedUser user = GetUser(client);
                MessagePacket messagePacket = (MessagePacket)disassembled;
                BroadcastPacket(packetBuilder.Build(messagePacket.ID, messagePacket.PacketData));
                Console.WriteLine($"[{client.ID}][{user.Nickname}]: {messagePacket.Text}");
            }
            if(packType.Equals(typeof(NicknamePacket)))
            {
                NicknamePacket nicknamePacket = (NicknamePacket)disassembled;
                Console.WriteLine(nicknamePacket.Nickname);
                if (!HasUser(nicknamePacket.Nickname))
                {
                    AddUser(client, nicknamePacket.Nickname);
                    realWordsController.AddUser(GetUser(client));
                }
                else
                {
                    Disconnect(client.ID);
                }
            }
            if(packType.Equals(typeof(UserIsReadyPacket)))
            {
                UserIsReadyPacket userIsReadyPacket = (UserIsReadyPacket)disassembled;
                if(HasUser(client))
                {
                    AuthorizedUser user = GetUser(client);
                    user.isReady = userIsReadyPacket.isReady;
                    RealWordsUser u;
                    if ((u = realWordsController.GetUser(user)) != null)
                    {
                        u.health = Config.StartHealth;
                    }
                    BroadcastPacket(packetBuilder.Build((int)PacketUtils.PacketIds.Message, Encoding.UTF8.GetBytes($"User '{user.Nickname}' is ready!")));
                    realWordsController.IncomingPacket(user, disassembled);
                }
            }
            if(packType.Equals(typeof(WordAnswer)))
            {
                if(HasUser(client))
                {
                    AuthorizedUser user = GetUser(client);
                    if(!user.isAnswering)
                    {
                        user.socket.Send(packetBuilder.Build((int)PacketUtils.PacketIds.Message, Encoding.UTF8.GetBytes("Fuck you")));
                        return;
                    }
                    if (realWordsController.HasUser(user))
                        realWordsController.IncomingPacket(user, disassembled);
                    else
                    {
                        Disconnect(client.ID);
                        Console.WriteLine("[ERR:0x0521312]");
                    }
                }
            }
            //Console.WriteLine(ByteBuilder.ToString(packet));
        }

        public void Println(string text, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}