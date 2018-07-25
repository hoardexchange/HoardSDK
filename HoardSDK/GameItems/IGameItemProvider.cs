using Hoard.BC;
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
        /// 
        /// </summary>
        string Symbol { get; }

        /// <summary>
        /// 
        /// </summary>
        GameItemContract Contract { get; }

        //FIXME: Is BCComm useful here?
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bcComm"></param>
        /// <param name="gameItem"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        Task<bool> Transfer(BCComm bcComm, GameItem gameItem, string senderAddress, string recipientAddress);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ownerAddress"></param>
        /// <returns></returns>
        Task<ulong> GetBalanceOf(string ownerAddress);

        /// <summary>
        /// Returns game items owned by the player. Synchronous function.
        /// Warning: might take long time to execute and return big number of items.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        GameItem[] GetGameItems(PlayerID player);

        /// <summary>
        /// Returns game item properties. Synchronous function.
        /// Warning: might take long time to execute.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        ItemProps GetGameItemProperties(GameItem item);
    }
}
