using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.BackendConnectors
{
    public interface IBackendConnector
    {
        string[] GetItemTypes(GameID game);
        GameItem[] GetPlayerItems(PlayerID playerID, string type);
        Task<bool> Transfer(PlayerID recipient, GameItem item);
    }
}
