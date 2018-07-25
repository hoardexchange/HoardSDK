using Hoard.BC;
using Hoard.BC.Contracts;
using System.Threading.Tasks;

namespace Hoard.GameAssets
{
    /// <summary>
    /// 
    /// </summary>
    public interface IGameAssetProvider
    {
        /// <summary>
        /// 
        /// </summary>
        string AssetSymbol { get; }

        /// <summary>
        /// 
        /// </summary>
        GameAssetContract Contract { get; }

        //FIXME: Is BCComm useful here?
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bcComm"></param>
        /// <param name="gameAsset"></param>
        /// <param name="address"></param>
        /// <returns></returns>
        Task<bool> Transfer(BCComm bcComm, GameAsset gameAsset, string senderAddress, string recipientAddress);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ownerAddress"></param>
        /// <returns></returns>
        Task<ulong> GetBalanceOf(string ownerAddress);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bcComm"></param>
        /// <param name="ownerAddress"></param>
        /// <returns></returns>
        Task<GameAsset[]> GetItems(BCComm bcComm, string ownerAddress);
    }
}
