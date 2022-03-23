using System;
using ABSoftware.ServerFiles.Users;

namespace ABSoftware.ServerFiles.Games
{
    public class RealWordsUser
    {
        public AuthorizedUser user;
        public int health;

        public RealWordsUser(AuthorizedUser user)
        {
            this.user = user;
        }
    }
}
