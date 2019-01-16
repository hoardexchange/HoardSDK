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
        public const string ABI = HoardABIConfig.HoardGameABI;

        private readonly Web3 web3;
        private Contract contract;

        public string Address { get { return contract.Address; } }

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

        //FIXME?
        //private Function GetFunctionPayoutPlayerReward()
        //{
        //    return contract.GetFunction("payoutPlayerReward");
        //}

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

        public Task<string> GetGameServerURLAsync()
        {
            var function = GetFunctionGameSrvURL();
            return function.CallAsync<string>();
        }

        public async Task<TransactionReceipt> SetGameServerURLAsync(string url, AccountInfo account)
        {
            var function = GetFunctionSetGameSrvURL();

            return await BCComm.EvaluateOnBC(web3, account, function, url);
        }

        public Task<ulong> GetGameItemContractCountAsync()
        {
            var function = GetFunctionNextItemIndex();
            return function.CallAsync<ulong>();
        }
        
        public Task<BigInteger> GetGameItemIdByIndexAsync(ulong gameIdx)
        {
            var function = GetFunctionGetItemIdByIndex();
            return function.CallAsync<BigInteger>(gameIdx);
        }

        public Task<string> GetGameItemContractAsync(BigInteger gameId)
        {
            var function = GetFunctionGetItemContract();
            return function.CallAsync<string>(gameId);
        }

        //FIXME?
        //public async Task<bool> PayoutPlayerReward(string tokenAddress, ulong amount, string from)
        //{
        //    var function = GetFunctionPayoutPlayerReward();
        //    var gas = await function.EstimateGasAsync(from, new Nethereum.Hex.HexTypes.HexBigInteger(100000), new Nethereum.Hex.HexTypes.HexBigInteger(0), tokenAddress, amount);
        //    gas = new Nethereum.Hex.HexTypes.HexBigInteger(gas.Value * 2);
        //    var receipt = await function.SendTransactionAndWaitForReceiptAsync(from, gas, new Nethereum.Hex.HexTypes.HexBigInteger(0), null, tokenAddress, amount);
        //    return receipt.Status.Value == 1;
        //}

        public Task<string> GetName()
        {
            var function = GetFunctionName();
            return function.CallAsync<string>();
        }

        public Task<string> GetDevName()
        {
            var function = GetFunctionDevName();
            return function.CallAsync<string>();
        }

        public Task<string> GetOwner()
        {
            var function = GetFunctionOwner();
            return function.CallAsync<string>();
        }
    }
}

