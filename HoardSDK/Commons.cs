using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// User account basic information.
    /// </summary>
    public class AccountInfo
    {
        public string ID { get; private set; } = null;
        public string Name { get; private set; } = null;
        public IAccountService AccountService { get; private set; } = null;

        public AccountInfo(string name, string id, IAccountService service)
        {
            Name = name;
            ID = id.ToLower();
            AccountService = service;
        }

        public override int GetHashCode() 
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AccountInfo);
        }

        public bool Equals(AccountInfo obj)
        {
            return obj != null && obj.ID.ToLower() == ID;
        }

        public async Task<string> Sign(byte[] input)
        {
            return await AccountService.Sign(input, this);
        }
    }

    /// <summary>
    /// Hoard user.
    /// </summary>
    public class User
    {
        public string UserName { get; private set; } = "";
        public List<AccountInfo> Accounts = new List<AccountInfo>();
        public AccountInfo ActiveAccount { get; private set; } = null;

        public User(string name)
        {
            UserName = name;
        }

        public bool SetActiveAccount(AccountInfo account)
        {
            if (Accounts.Contains(account))
            {
                ActiveAccount = account;
                return true;
            }

            System.Diagnostics.Debug.Fail("Invalid parameter. This account is not verified!");
            
            return false;
        }
    }

    /// <summary>
    /// Game identifier.
    /// </summary>
    public class GameID : IEquatable<GameID>
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public string GameOwner { get; set; }

        public static GameID kInvalidID { get; private set; } = new GameID(null);

        public GameID(string id)
        {
            ID = id;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public bool Equals(GameID other)
        {
            if (other == null)
                return false;

            if (ID == other.ID)
                return true;
            else
                return false;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            GameID gameObj = obj as GameID;
            if (gameObj == null)
                return false;
            else
                return Equals(gameObj);
        }

        public static bool operator ==(GameID game1, GameID game2)
        {
            if (((object)game1) == null || ((object)game2) == null)
                return Object.Equals(game1, game2);

            return game1.Equals(game2);
        }

        public static bool operator !=(GameID game1, GameID game2)
        {
            if (((object)game1) == null || ((object)game2) == null)
                return !Object.Equals(game1, game2);

            return !(game1.Equals(game2));
        }
    }
}
