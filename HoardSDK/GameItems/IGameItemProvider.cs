using Hoard.BC.Contracts;
using System.Threading.Tasks;

namespace Hoard.GameItems
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGameItemProvider
    {
        /// <summary>
        /// Unique identificator of game items.
        /// </summary>
        string Symbol { get; }

        /// <summary>
        /// 
        /// </summary>
        GameItemContract Contract { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="recipient"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        Task<bool> Transfer(PlayerID recipient, GameItem item);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        Task<ulong> GetBalanceOf(PlayerID player);

        /// <summary>
        /// Returns game items owned by the player. Synchronous function.
        /// Warning: might take long time to execute and return big number of items.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        GameItem[] GetGameItems(PlayerID player);

        /// <summary>
        /// Updates game item properties and updates game item. Synchronous function.
        /// Warning: might take long time to execute.
        /// </summary>
        void UpdateGameItemProperties(GameItem item);
    }
}
