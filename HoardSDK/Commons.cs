using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    public class PlayerID
    {
        public string ID;

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
}
