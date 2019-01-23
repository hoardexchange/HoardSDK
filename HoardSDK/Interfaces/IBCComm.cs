using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.Interfaces
{
    public interface IBCComm
    {
        /// <summary>
        /// Connects to blockchain
        /// </summary>
        /// <returns>a pair of [bool result, string return infromation] received from client</returns>
        Task<Tuple<bool, string>> Connect();

        /// <summary>
        /// Returns ETH balance of given account
        /// </summary>
        /// <param name="account">account to query</param>
        /// <returns></returns>
        Task<BigInteger> GetBalance(HoardID account);

        /// <summary>
        /// Returns HRD balance of given account
        /// </summary>
        /// <param name="account">account to query</param>
        /// <returns></returns>
        Task<BigInteger> GetHRDBalance(HoardID account);

        /// <summary>
        /// Registers existing Hoard game. This game must exist on Hoard platform.
        /// This function performs initial setup of game contract.
        /// </summary>
        /// <param name="game">[in/out] game object must contain valid ID. Other fields will be retrieved from platform</param>
        /// <returns></returns>
        Task<bool> RegisterHoardGame(GameID game);

        /// <summary>
        /// Removes game from system. Call when you are finished with using that game
        /// </summary>
        /// <param name="game">game to unregister</param>
        void UnregisterHoardGame(GameID game);

        /// <summary>
        /// Returns all registered games (using RegisterHoardGame)
        /// </summary>
        /// <returns></returns>
        GameID[] GetRegisteredHoardGames();

        /// <summary>
        /// Retrieves all Hoard games registered on the platform.
        /// </summary>
        /// <returns></returns>
        Task<GameID[]> GetHoardGames();

        /// <summary>
        /// Checks if game is registered on Hoard Platform
        /// </summary>
        /// <param name="gameID">game ID to check</param>
        /// <returns></returns>
        Task<bool> GetGameExists(BigInteger gameID);

        /// <summary>
        /// Returns address of Hoard exchange contract
        /// </summary>
        /// <returns></returns>
        Task<string> GetHoardExchangeContractAddress();
    }
}
