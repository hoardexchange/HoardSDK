using Hoard.GameItems;
using System.Collections.Generic;

namespace Hoard
{
    /// <summary>
    /// Game provider interface.
    /// </summary>
    public interface IProvider
    {
        // TODO: add method that enables pagination/filtering of items

        /// <summary>
        /// Returns the game items owned by a player from all registered IGameItemProviders. 
        /// Synchronous function.
        /// Warning: might take long time to execute and return big number of items.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        GameItem[] GetGameItems(PlayerID player);

        /// <summary>
        /// Returns the game item properties. Synchronous function.
        /// Warning: might take long time to execute.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        ItemProps GetGameItemProperties(GameItem item);

        /// <summary>
        /// Returns provider for given game item.
        /// </summary>
        /// <returns></returns>
        IGameItemProvider GetGameItemProvider(GameItem item);
    }
}
