using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Player identifier. Holds only ID information.
    /// </summary>
    public class PlayerID // FIXME: should be renamed to UserAccount?
    {
        public static PlayerID kInvalidID { get; private set; } = new PlayerID("0x0","0x0","");
        public string ID { get; private set; } = null;
        // FIXME: probably we shouldn't store private key or password here
        public string PrivateKey { get; private set; }
        public string Password { get; private set; }

        public PlayerID(string id, string privateKey, string password)
        {
            ID = id.ToLower();
            PrivateKey = privateKey;
            Password = password;
        }

        public override int GetHashCode() 
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PlayerID);
        }

        public bool Equals(PlayerID obj)
        {
            return obj != null && obj.ID.ToLower() == ID && obj.PrivateKey == PrivateKey && obj.Password == Password;
        }

    }

    /// <summary>
    /// Game identifier.
    /// </summary>
    public class GameID : IEquatable<GameID>
    {
        public string ID;
        public string Name;
        public string Url;

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
