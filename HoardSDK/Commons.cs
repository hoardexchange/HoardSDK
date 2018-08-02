using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    public class PlayerID
    {
        public static PlayerID kInvalidID { get; private set; } = new PlayerID("0x0","0x0");
        public string ID { get; private set; } = null;
        public string PrivateKey { get; private set; }

        public PlayerID(string id, string key)
        {
            ID = id.ToLower();
            PrivateKey = key;
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
            return obj != null && obj.ID.ToLower() == ID && obj.PrivateKey == PrivateKey;
        }

    }

    public class GameID
    {
        public string ID;
        public string Name;
        public string Url;

        public static GameID kInvalidID { get; private set; } = new GameID("");

        public GameID(string id)
        {
            ID = id;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GameID);
        }

        public bool Equals(GameID obj)
        {
            return obj != null && obj.ID == ID;
        }

    }
}
