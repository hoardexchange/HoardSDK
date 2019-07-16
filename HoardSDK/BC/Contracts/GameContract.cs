using Hoard.Exceptions;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Diagnostics;
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

        private Function GetFunctionGameServerURL()
        {
            return contract.GetFunction("gameServerURL");
        }

        private Function GetFunctionSetGameServerURL()
        {
            return contract.GetFunction("setGameServerURL");
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

        private Function GetFunctionSymbol()
        {
            return contract.GetFunction("symbol");
        }

        private Function GetFunctionOwner()
        {
            return contract.GetFunction("owner");
        }

        private Function GetFunctionAddGameItemContract()
        {
            return contract.GetFunction("addGameItemContract");
        }

        private Function GetFunctionAddAdmin()
        {
            return contract.GetFunction("addAdmin");
        }

        private Function GetFunctionRemoveAdmin()
        {
            return contract.GetFunction("removeAdmin");
        }

        /// <summary>
        /// Returns Game Server URL stored within contract
        /// </summary>
        /// <returns></returns>
        public Task<string> GetGameServerURLAsync()
        {
            var function = GetFunctionGameServerURL();
            return function.CallAsync<string>();
        }

        /// <summary>
        /// Sets new Game Server URL in contract
        /// </summary>
        /// <param name="url">new URL of Game Server</param>
        /// <param name="profile">signer profile</param>
        /// <returns>receipt of the transaction</returns>
        public async Task<TransactionReceipt> SetGameServerURLAsync(string url, Profile profile)
        {
            var function = GetFunctionSetGameServerURL();
            return await BCComm.EvaluateOnBC(web3, profile, function, url);
        }

        /// <summary>
        /// Adds game item contract address
        /// </summary>
        /// <param name="address">Item contract address</param>
        /// <param name="profile">signer profile</param>
        /// <returns></returns>
        public async Task<TransactionReceipt> AddGameItemAsync(string address, Profile profile)
        {
            var function = GetFunctionAddGameItemContract();
            return await BCComm.EvaluateOnBC(web3, profile, function, address);
        }

        /// <summary>
        /// Adds admin to game contract
        /// </summary>
        /// <param name="adminAddr">Admin address</param>
        /// <param name="profile">signer profile</param>
        /// <returns></returns>
        public async Task<TransactionReceipt> AddAdminAsync(string adminAddr, Profile profile)
        {
            if (profile.ID == adminAddr)
            {
                throw new HoardException("Can't add my self");
            }
            var function = GetFunctionAddAdmin();
            return await BCComm.EvaluateOnBC(web3, profile, function, adminAddr);
        }

        /// <summary>
        /// Removes admin from game contract
        /// </summary>
        /// <param name="adminAddr">Admin address</param>
        /// <param name="profile">signer profile</param>
        /// <returns></returns>
        public async Task<TransactionReceipt> RemoveAdminAsync(string adminAddr, Profile profile)
        {
            if (profile.ID == adminAddr)
            {
                throw new HoardException("Can't remove my self");
            }
            var function = GetFunctionRemoveAdmin();
            return await BCComm.EvaluateOnBC(web3, profile, function, adminAddr);
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
        /// Returns game symbol (unique name identifier)
        /// </summary>
        /// <returns></returns>
        public Task<string> GetSymbol()
        {
            var function = GetFunctionSymbol();
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

