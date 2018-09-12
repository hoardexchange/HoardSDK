using Hoard.BC.Contracts;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Game items params used by IGameItemProvider.GetItems 
    /// </summary>
    public class GameItemsParams
    {
        public PlayerID PlayerID = null;
        public string ContractAddress = null;
        public BigInteger TokenId = 0;
    }

    /// <summary>
    /// Provider for Game Items interface.
    /// </summary>
    public interface IGameItemProvider
    {
        /// <summary>
        /// Game this provider supports
        /// </summary>
        GameID Game { get; }

        /// <summary>
        /// Returns all types supported by this provider
        /// </summary>
        /// <returns></returns>
        string[] GetItemTypes();

        /// <summary>
        /// Returns all items belonging to a particular player
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        GameItem[] GetPlayerItems(PlayerID playerID);
        /// <summary>
        /// Returns all items belonging to a particular player with given type
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        GameItem[] GetPlayerItems(PlayerID playerID, string itemType);

        /// Retrieve all items matching given parameters
        /// </summary>
        /// <param name="gameItemsParams"></param>
        /// <returns></returns>
        GameItem[] GetItems(GameItemsParams[] gameItemsParams);

        /// <summary>
        /// Changes ownership of an item, sending it to new owner
        /// </summary>
        /// <param name="addressFrom"></param>
        /// <param name="addressTo"></param>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        Task<bool> Transfer(string addressFrom, string addressTo, GameItem item, ulong amount);

        /// <summary>
        /// Initializes provider (connects to backend)
        /// </summary>
        /// <returns></returns>
        bool Connect();
    }
}
