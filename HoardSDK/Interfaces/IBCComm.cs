using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.Interfaces
{
    public interface IBCComm
    {
        Task<Tuple<bool, string>> Connect();

        Task<BigInteger> GetBalance(HoardID account);
        Task<BigInteger> GetHRDBalance(HoardID account);

        Task<string> GetHRDAddress(); //TODO probably not needed here

        Task<bool> RegisterHoardGame(GameID game);
        void UnregisterHoardGame(GameID game);
        GameID[] GetRegisteredHoardGames();
        Task<GameID[]> GetHoardGames();
        Task<bool> GetGameExists(BigInteger gameID);
        Task<string> GetHoardExchangeContractAddress();
    }
}
