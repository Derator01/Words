using System;
using System.Text;
using System.IO;
using ABSoftware;
using ABSoftware.ServerFiles;
using ABSoftware.ServerFiles.Utils;

namespace WordsGameServer
{
    class Program : Server
    {
        public static Program instance;
        ByteBuilder bb = new ByteBuilder();

        public static int port = 8921;

        static void Main(string[] args)
        {
            Program p = new Program();
            instance = p;
            p.Start(port);
        }

        public override void OnServerStart()
        {
            Println("The server is running on port " + Port, ConsoleColor.Cyan);
        }

        public override void OnServerStop()
        {
            Println("The server was successfully stopped!", ConsoleColor.Cyan);
        }

        public override void OnClientConnect(Client client)
        {
            Console.WriteLine($"Client {client.ID} has connected from {client.IP}!");
        }

        public override void OnClientDisconnect(Client client)
        {
            Console.WriteLine($"Client {client.ID} has disconnected from the server!");
        }

        public override void OnConsoleInput(string input)
        {
            switch(input.ToLower())
            {
                case "start":
                    if(!Start(port))
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
                case "shit":
                    {
                        BroadcastPacket(packetBuilder.Build(0, new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }));
                    }
                    break;
                default:
                    Console.WriteLine("Command not found!");
                    break;
            }
        }

        public override void OnIncomingPacket(Client client, byte[] packet)
        {
            Console.WriteLine(bb.ToString(packet));
        }

        public void Println(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}