using Hoard.BC.Contracts;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Game items params used for filtering queries by IGameItemProvider.GetItems 
    /// </summary>
    public class GameItemsParams
    {
        /// <summary>
        /// public address of player
        /// </summary>
        public string PlayerAddress = null;
        /// <summary>
        /// address of GameItem contract (type of GameITem)
        /// </summary>
        public string ContractAddress = null;
        /// <summary>
        /// TODO: what is it used for???
        /// </summary>
        public BigInteger Amount = BigInteger.Zero;
        /// <summary>
        /// TokenID to query (for ERC721 tokens only)
        /// </summary>
        public string TokenId = null;
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
        /// <param name="account"></param>
        /// <returns></returns>
        Task<GameItem[]> GetPlayerItems(AccountInfo account);

        /// <summary>
        /// Returns all items belonging to a particular player with given type
        /// </summary>
        /// <param name="account"></param>
        /// <param name="itemType"></param>
        /// <returns></returns>
        Task<GameItem[]> GetPlayerItems(AccountInfo account, string itemType);

        /// <summary>
        /// Retrieve all items matching given parameters
        /// </summary>
        /// <param name="gameItemsParams"></param>
        /// <returns></returns>
        Task<GameItem[]> GetItems(GameItemsParams[] gameItemsParams);

        /// <summary>
        /// Changes ownership of an item, sending it to new owner
        /// </summary>
        /// <param name="addressFrom">sender address</param>
        /// <param name="addressTo">receiver address</param>
        /// <param name="item">item to transfer</param>
        /// <param name="amount">amount of itmes to transfer (for NFT it must be equal to 1)</param>
        /// <returns></returns>
        Task<bool> Transfer(string addressFrom, string addressTo, GameItem item, ulong amount);

        /// <summary>
        /// Initializes provider (connects to backend)
        /// </summary>
        /// <returns></returns>
        Task<bool> Connect();
    }
}
