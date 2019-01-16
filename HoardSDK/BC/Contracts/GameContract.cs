using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.BC.Contracts
{
    /// <summary>
    /// Hoard Game contract with list of all supported game item types (other contracts)
    /// </summary>
    public class GameContract
    {
        /// <summary>
        /// Application Binary Interface for a contract
        /// </summary>
        public const string ABI = HoardABIConfig.HoardGameABI;
        /// <summary>
        /// ETH address of this contract
        /// </summary>
        public string Address { get { return contract.Address; } }

        private readonly Web3 web3;
        private Contract contract;

        /// <summary>
        /// Creates new Game contract
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="address">address of this contract (u160)</param>
        public GameContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        private Function GetFunctionNextItemIndex()
        {
            return contract.GetFunction("nextItemIndex");
        }

        private Function GetFunctionGameSrvURL()
        {
            return contract.GetFunction("gameSrvURL");
        }

        private Function GetFunctionSetGameSrvURL()
        {
            return contract.GetFunction("setGameSrvURL");
        }

        private Function GetFunctionGetItemIdByIndex()
        {
            return contract.GetFunction("itemIdsMap");
        }

        private Function GetFunctionGetItemContract()
        {
            return contract.GetFunction("itemContractMap");
        }

        private Function GetFunctionName()
        {
            return contract.GetFunction("name");
        }

        private Function GetFunctionDevName()
        {
            return contract.GetFunction("devName");
        }

        private Function GetFunctionOwner()
        {
            return contract.GetFunction("owner");
        }

        /// <summary>
        /// Returns Game Server URL stored within contract
        /// </summary>
        /// <returns></returns>
        public Task<string> GetGameServerURLAsync()
        {
            var function = GetFunctionGameSrvURL();
            return function.CallAsync<string>();
        }

        /// <summary>
        /// Sets new Game Server URL in contract
        /// </summary>
        /// <param name="url">new URL of Game Server</param>
        /// <param name="account">signer account</param>
        /// <returns>receipt of the transaction</returns>
        public async Task<TransactionReceipt> SetGameServerURLAsync(string url, AccountInfo account)
        {
            var function = GetFunctionSetGameSrvURL();

            return await BCComm.EvaluateOnBC(web3, account, function, url);
        }

        /// <summary>
        /// Returns number of GameItems supported natively by this game
        /// </summary>
        /// <returns></returns>
        public Task<ulong> GetGameItemContractCountAsync()
        {
            var function = GetFunctionNextItemIndex();
            return function.CallAsync<ulong>();
        }
        
        /// <summary>
        /// Returns GameItem ID based on index
        /// </summary>
        /// <param name="gameIdx">index of GameItem</param>
        /// <returns></returns>
        public Task<BigInteger> GetGameItemIdByIndexAsync(ulong gameIdx)
        {
            var function = GetFunctionGetItemIdByIndex();
            return function.CallAsync<BigInteger>(gameIdx);
        }

        /// <summary>
        /// Get address of Game Item contract
        /// </summary>
        /// <param name="gameItemId">ID of the game item. <see cref="GetGameItemIdByIndexAsync(ulong)"/></param>
        /// <returns></returns>
        public Task<string> GetGameItemContractAsync(BigInteger gameItemId)
        {
            var function = GetFunctionGetItemContract();
            return function.CallAsync<string>(gameItemId);
        }

        /// <summary>
        /// Returns full name of this game
        /// </summary>
        /// <returns></returns>
        public Task<string> GetName()
        {
            var function = GetFunctionName();
            return function.CallAsync<string>();
        }

        /// <summary>
        /// Returns developer inner name of this game (ID is based on this name)
        /// </summary>
        /// <returns></returns>
        public Task<string> GetDevName()
        {
            var function = GetFunctionDevName();
            return function.CallAsync<string>();
        }

        /// <summary>
        /// Returns owner account of this game (address)
        /// </summary>
        /// <returns></returns>
        public Task<string> GetOwner()
        {
            var function = GetFunctionOwner();
            return function.CallAsync<string>();
        }
    }
}

