using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.BC.Contracts
{
    /// <summary>
    /// Main Hoard contract with a list of all games registered on platform. Central point from which we can get all neccessery data.
    /// </summary>
    public class GameCenterContract
    {
        private const string ABI = HoardABIConfig.HoardGameCenterABI;

        private readonly Web3 web3;
        private Contract contract;

        /// <summary>
        /// Returns address of this contract
        /// </summary>
        public string Address { get { return contract.Address; } }

        /// <summary>
        /// Creates new game cetner contract access object
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="address">address of deployed contract</param>
        public GameCenterContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        private Function GetFunctionOwner()
        {
            return contract.GetFunction("owner");
        }

        private Function GetFunctionName()
        {
            return contract.GetFunction("name");
        }

        private Function GetFunctionGetGameContract()
        {
            return contract.GetFunction("getGameContract");
        }

        private Function GetFunctionGetGameIdByIndex()
        {
            return contract.GetFunction("getGameIdByIndex");
        }

        private Function GetFunctionGameExists()
        {
            return contract.GetFunction("gameExists");
        }

        private Function GetFunctionAddGame()
        {
            return contract.GetFunction("addGame");
        }

        private Function GetFunctionAddAdmin()
        {
            return contract.GetFunction("addAdmin");
        }

        private Function GetFunctionRemoveAdmin()
        {
            return contract.GetFunction("removeAdmin");
        }

        private Function GetFunctionExchangeAddress()
        {
            return contract.GetFunction("exchangeAddress");
        }

        private Function GetFunctionSetExchangeAddress()
        {
            return contract.GetFunction("setExchangeAddress");
        }

        private Function GetFunctionExchangeSrvURL()
        {
            return contract.GetFunction("exchangeSrvURL");
        }

        private Function GetFunctionHoardTokenAddress()
        {
            return contract.GetFunction("hoardTokenAddress");
        }

        private Function GetFunctionSetHoardTokenAddress()
        {
            return contract.GetFunction("setHoardTokenAddress");
        }

        /// <summary>
        /// Returns game contract address by id
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public Task<string> GetGameContractAsync(BigInteger gameID)
        {
            var function = GetFunctionGetGameContract();
            return function.CallAsync<string>(gameID);
        }

        /// <summary>
        /// Returns game id address by index
        /// </summary>
        /// <param name="index">index of the game</param>
        /// <returns></returns>
        public Task<BigInteger> GetGameIdByIndexAsync(ulong index)
        {
            var function = GetFunctionGetGameIdByIndex();
            return function.CallAsync<BigInteger>(index);
        }

        /// <summary>
        /// Returns total number of registered games on Hoard Platform
        /// </summary>
        /// <returns></returns>
        public Task<ulong> GetGameCount()
        {
            Function function = contract.GetFunction("nextGameIndex");
            return function.CallAsync<ulong>();
        }

        /// <summary>
        /// Checks if game exists
        /// </summary>
        /// <param name="gameID">ID of the game</param>
        /// <returns></returns>
        public Task<bool> GetGameExists(BigInteger gameID)
        {
            var function = GetFunctionGameExists();
            return function.CallAsync<bool>(gameID);
        }

        /// <summary>
        /// Adds new game to game center
        /// </summary>
        /// <param name="gameAddr">addres of game contract</param>
        /// <param name="profile">profile that will pay for transaction</param>
        /// <returns></returns>
        public async Task<TransactionReceipt> AddGame(string gameAddr, Profile profile)
        {
            var function = GetFunctionAddGame();
            return await BCComm.EvaluateOnBC(web3, profile, function, gameAddr);
        }

        /// <summary>
        /// Adds user as an administrator of the contract
        /// </summary>
        /// <param name="adminAddr">addres of user</param>
        /// <param name="profile">profile that will pay for transaction</param>
        /// <returns></returns>
        public async Task<TransactionReceipt> AddAdmin(string adminAddr, Profile profile)
        {
            var function = GetFunctionAddAdmin();
            return await BCComm.EvaluateOnBC(web3, profile, function, adminAddr);
        }

        /// <summary>
        /// Removes administrator privilages from existing user
        /// </summary>
        /// <param name="adminAddr">addres of user</param>
        /// <param name="profile">profile that will pay for transaction</param>
        /// <returns></returns>
        public async Task<TransactionReceipt> RemoveAdminAsync(string adminAddr, Profile profile)
        {
            var function = GetFunctionRemoveAdmin();
            return await BCComm.EvaluateOnBC(web3, profile, function, adminAddr);
        }

        /// <summary>
        /// Sets contract to be the Exchange contract
        /// </summary>
        /// <param name="address">Exchange contract address</param>
        /// <param name="profile">payer</param>
        /// <returns></returns>
        public async Task<TransactionReceipt> SetExchangeAddress(string address, Profile profile)
        {
            var function = GetFunctionSetExchangeAddress();
            return await BCComm.EvaluateOnBC(web3, profile, function, address);
        }

        /// <summary>
        /// Sets contract to be the Hoard Token
        /// </summary>
        /// <param name="address">Hoard Token contract address</param>
        /// <param name="profile">payer</param>
        /// <returns></returns>
        public async Task<TransactionReceipt> SetHoardTokenAddress(string address, Profile profile)
        {
            var function = GetFunctionSetHoardTokenAddress();
            return await BCComm.EvaluateOnBC(web3, profile, function, address);
        }

        /// <summary>
        /// Returns Exchange contract address
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetExchangeAddress()
        {
            var function = GetFunctionExchangeAddress();
            return await function.CallAsync<string>();
        }

        /// <summary>
        /// Returns Hoard Exchange server URL
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetExchangeSrvURL()
        {
            var function = GetFunctionExchangeSrvURL();
            return await function.CallAsync<string>();
        }

        /// <summary>
        /// Returns Hoard Token contract address
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetHoardTokenAddressAsync()
        {
            var function = GetFunctionHoardTokenAddress();
            return await function.CallAsync<string>();
        }

        /// <summary>
        /// Returns owner of this contract
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetContractOwner()
        {
            var function = GetFunctionOwner();
            return await function.CallAsync<string>();
        }
    }
}
