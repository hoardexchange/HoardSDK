using Hoard.Utils;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Account basic information.
    /// </summary>
    public class AccountInfo
    {
        public static AccountInfo kInvalidID { get; private set; } = new AccountInfo("0x0","0x0","", null);
        public string ID { get; private set; } = null;
        public string PrivateKey { get; private set; } = "";
        public string Password { get; private set; } = "";
        public Account BCAccount;
        public IAccountService AccountService { get; private set; } = null;

        public AccountInfo(string id, string privateKey, string password, IAccountService service)
        {
            ID = id.ToLower();
            PrivateKey = privateKey;
            Password = password;
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
            return obj != null && obj.ID.ToLower() == ID && obj.PrivateKey == PrivateKey && obj.Password == Password;
        }

    }

    /// <summary>
    /// Hoard user.
    /// </summary>
    public class User
    {
        public enum ServiceType
        {
            KeyContainer = 0,
            MaxServiceTypes
        }

        public string UserName { get; private set; } = "";
        public string HashedUsername { get; private set; } = "";
        public string Password { get; private set; } = "";
        public AccountInfo ActiveAccount { get; private set; } = null;
        public IAccountService[] AccountServices = new IAccountService[(int)ServiceType.MaxServiceTypes];

        public User(string name, string password)
        {
            UserName = name;
            Password = password;
            HashedUsername = Helper.SHA256HexHashString(name);

            AccountServices[(int)ServiceType.KeyContainer] = new KeyContainerService();
        }

        public bool SetActiveAccount(AccountInfo account)
        {
            foreach(IAccountService service in AccountServices)
            {
                List<AccountInfo> accounts = service.GetAccounts();
                foreach(AccountInfo ac in accounts)
                {
                    if(ac.ID == account.ID)
                    {
                        ActiveAccount = ac;
                        return true;
                    }
                }
            }
            
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
