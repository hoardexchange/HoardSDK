using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.BC.Contracts
{
    [FunctionOutput]
    class GameInfoDTO
    {
        [Parameter("uint64", "id", 1)]
        public ulong ID { get; set; }

        [Parameter("bytes32", "name", 2)]
        public string Name { get; set; }

    }

    /// <summary>
    /// Main Hoard contract with a list of all games registered on platform. Central point from which we can get all neccessery data.
    /// </summary>
    internal class GameCenterContract
    {
        public const string ABI = HoardABIConfig.HoardGameCenterABI;

        private readonly Web3 web3;
        private Contract contract;

        public string Address { get { return contract.Address; } }

        public GameCenterContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        public Function GetFunctionOwner()
        {
            return contract.GetFunction("owner");
        }

        public Function GetFunctionName()
        {
            return contract.GetFunction("name");
        }

        public Function GetFunctionGetGameContract()
        {
            return contract.GetFunction("getGameContract");
        }

        public Function GetFunctionGetGameIdByIndex()
        {
            return contract.GetFunction("getGameIdByIndex");
        }

        public Function GetFunctionGameExists()
        {
            return contract.GetFunction("gameExists");
        }

        public Function GetFunctionAddGame()
        {
            return contract.GetFunction("addGame");
        }

        public Function GetFunctionAddAdmin()
        {
            return contract.GetFunction("addAdmin");
        }

        public Function GetFunctionRemoveAdmin()
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

        public Task<bool> GetGameExistsAsync(BigInteger gameID)
        {
            var function = GetFunctionGameExists();
            return function.CallAsync<bool>(gameID);
        }

        public async Task<TransactionReceipt> AddGameAsync(string gameAddr, Profile profile)
        {
            var function = GetFunctionAddGame();
            return await BCComm.EvaluateOnBC(web3, profile, function, gameAddr);
        }

        public async Task<TransactionReceipt> AddAdminAsync(string adminAddr, Profile profile)
        {
            var function = GetFunctionAddAdmin();
            return await BCComm.EvaluateOnBC(web3, profile, function, adminAddr);
        }

        public async Task<TransactionReceipt> RemoveAdminAsync(string adminAddr, Profile profile)
        {
            var function = GetFunctionRemoveAdmin();
            return await BCComm.EvaluateOnBC(web3, profile, function, adminAddr);
        }

        public async Task<TransactionReceipt> SetExchangeAddressAsync(string address, Profile profile)
        {
            var function = GetFunctionSetExchangeAddress();
            return await BCComm.EvaluateOnBC(web3, profile, function, address);
        }

        public async Task<TransactionReceipt> SetHoardTokenAddressAsync(string address, Profile profile)
        {
            var function = GetFunctionSetHoardTokenAddress();
            return await BCComm.EvaluateOnBC(web3, profile, function, address);
        }

        public async Task<string> GetExchangeAddressAsync()
        {
            var function = GetFunctionExchangeAddress();
            return await function.CallAsync<string>();
        }

        public async Task<string> GetExchangeSrvURLAsync()
        {
            var function = GetFunctionExchangeSrvURL();
            return await function.CallAsync<string>();
        }

        public async Task<string> GetHoardTokenAddressAsync()
        {
            var function = GetFunctionHoardTokenAddress();
            return await function.CallAsync<string>();
        }

        public async Task<string> GetContractOwner()
        {
            var function = GetFunctionOwner();
            return await function.CallAsync<string>();
        }
    }
}
