using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    public class PlayerID
    {
        public static PlayerID kInvalidID { get; private set; } = new PlayerID("");
        public string ID = null;

        public PlayerID(string id)
        {
            ID = id.ToLower();
        }

        public static implicit operator PlayerID(string d)
        {
            return new PlayerID(d);
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
            return obj != null && obj.ID.ToLower() == ID;
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

        public static implicit operator GameID(string d)
        {
            return new GameID(d);
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
