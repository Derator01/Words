using System;
using System.IO;
using System.Threading;
using ABSoftware.ServerFiles.Users;
using ABSoftware.ServerFiles.PacketUtils.Packets;
using ABSoftware.ServerFiles.Utils;
using System.Text;

namespace ABSoftware.ServerFiles.Games
{
    public class RealWordsController : INetListener
    {
        private Server server;

        private Random rnd = new Random();
        private ArrayList<string> words = new ArrayList<string>();
        public ArrayList<RealWordsUser> users = new ArrayList<RealWordsUser>();

        public bool Awake { get; set; }

        int turnId = 0;
        WordData currentWord = default(WordData);

        public RealWordsController(Server server)
        {
            this.server = server;
            LoadWords();
            Awake = true;
        }

        public void IncomingPacket(AuthorizedUser user, Packet packet)
        {
            Type type = packet.GetType();
            if(type.Equals(typeof(UserIsReadyPacket)))
            {
                if(ReadyCount().Equals(Config.LobbySize)) //Start game
                {
                    GameStart();
                }
            }
            if(type.Equals(typeof(WordAnswer)))
            {
                WordAnswer wordAnswer = (WordAnswer)packet;
                if(words.Contains(wordAnswer.Word) && wordAnswer.Word.Contains(currentWord.GetChars()))
                {
                    //Guessed right
                    user.isAnswering = false;
                    server.BroadcastPacket(server.packetBuilder.Build((int)PacketUtils.PacketUtils.PacketIds.Message, Encoding.UTF8.GetBytes($"'{user.Nickname}' guessed right!")));
                    NextTurn();
                }
                else
                {
                    //Guessed wrong
                    RealWordsUser u = GetUser(user);
                    u.health--;
                    server.BroadcastPacket(server.packetBuilder.Build((int)PacketUtils.PacketUtils.PacketIds.Message, Encoding.UTF8.GetBytes($"'{user.Nickname}' guessed wrong! He has {u.health} HP")));
                    if (u.health <= 0)
                        u.user.socket.Send(server.packetBuilder.Build((int)PacketUtils.PacketUtils.PacketIds.Message, Encoding.UTF8.GetBytes($"'{user.Nickname}' lost")));
                    NextTurn();
                }
            }
        }

        void GameStart()
        {
            turnId = rnd.Next(0, ReadyCount());
            currentWord = RandomWord();
            SendWordData();
        }

        int NextTurn()
        {
            turnId++;
            if (turnId >= ReadyCount())
                turnId = 0;
            currentWord = RandomWord();
            SendWordData();
            return turnId;
        }

        void SendWordData(bool nextTurn = true)
        {
            RealWordsUser user = ReadyUsers()[turnId];
            user.user.isAnswering = true;
            if(nextTurn)
                server.BroadcastPacket(server.packetBuilder.Build((int)PacketUtils.PacketUtils.PacketIds.Message, Encoding.UTF8.GetBytes($"'{user.user.Nickname}' is now guessing!")));
            user.user.socket.Send(server.packetBuilder.Build((int)PacketUtils.PacketUtils.PacketIds.StartGuessing, Encoding.UTF8.GetBytes(currentWord.GetChars())));
        }

        private WordData RandomWord()
        {
            string word = words[rnd.Next(0, words.Size)];
            int firstLetter = rnd.Next(word.Length - 2);
            WordData data = new WordData() { Word = word, Chars = new char[] { word[firstLetter], word[firstLetter + 1], word[firstLetter + 2] } };
            return data;
        }

        private void LoadWords()
        {
            if (File.Exists(FilePaths.WORDS_PATH))
            {
                words = new ArrayList<string>(File.ReadAllLines(FilePaths.WORDS_PATH));
                Console.WriteLine($"Words are successfully initialized!");
            }
            else
                Console.WriteLine($"File {FilePaths.WORDS_PATH} does not exists!");
        }

        private int ReadyCount()
        {
            int count = 0;
            for (int i = 0; i < users.Size; i++)
                if (users[i].user.isReady) count++;

            return count;
        }

        private ArrayList<RealWordsUser> ReadyUsers()
        {
            ArrayList<RealWordsUser> newUsers = new ArrayList<RealWordsUser>();
            for (int i = 0; i < users.Size; i++)
                if (users[i].user.isReady && users[i].health > 0) newUsers.Add(users[i]);

            return newUsers;
        }

        private struct WordData
        {
            public string Word;
            public char[] Chars;
            public string GetChars() { return new string(Chars); }
        }

        #region RealWords User controls
        #region Has User
        public bool HasUser(AuthorizedUser user)
        {
            for (int i = 0; i < users.Size; i++)
            {
                if (users[i].user.socket.ID.Equals(user.socket.ID))
                    return true;
            }
            return false;
        }

        public bool HasUser(string nickname)
        {
            for (int i = 0; i < users.Size; i++)
            {
                if (users[i].user.Nickname.Equals(nickname))
                    return true;
            }
            return false;
        }
        #endregion
        #region Add User
        public bool AddUser(AuthorizedUser user)
        {
            if (!HasUser(user))
                users.Add(new RealWordsUser(user));
            return false;
        }
        #endregion
        #region Get User
        public RealWordsUser GetUser(AuthorizedUser user)
        {
            for (int i = 0; i < users.Size; i++)
            {
                if (users[i].user.socket.ID.Equals(user.socket.ID))
                    return users[i];
            }
            return null;
        }

        public RealWordsUser GetUser(string nickname)
        {
            for (int i = 0; i < users.Size; i++)
            {
                if (users[i].user.Nickname.Equals(nickname))
                    return users[i];
            }
            return null;
        }
        #endregion
        #region Remove User
        public bool RemoveUser(RealWordsUser user)
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
    }
}
