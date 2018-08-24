using Hoard.BC.Contracts;
using System.Threading.Tasks;

namespace Hoard
{
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

        /// <summary>
        /// Changes ownership of an item, sending it to new owner
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> Transfer(PlayerID recipient, GameItem item);

        /// <summary>
        /// Initializes provider (connects to backend)
        /// </summary>
        /// <returns></returns>
        bool Connect();
    }
}
