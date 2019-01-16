using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Transactions;
using Nethereum.RPC.NonceServices;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.BC.Contracts
{
    /// <summary>
    /// Hoard Game contract with list of all supported game item types (other contracts)
    /// </summary>
    internal class HoardTokenContract
    {
        public const string ABI = HoardABIConfig.ERC223TokenMockABI;

        private readonly Web3 web3;
        private Contract contract;

        public string Address { get { return contract.Address; } }

        public HoardTokenContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        private Function GetFunctionBalanceOf()
        {
            return contract.GetFunction("balanceOf");
        }

        private Function GetFunctionTransfer()
        {
            return contract.GetFunction("transfer");
        }

        public Task<BigInteger> GetBalanceOf(string address)
        {
            var function = GetFunctionBalanceOf();
            return function.CallAsync<BigInteger>(address);
        }

        public async Task<bool> Transfer(AccountInfo from, string to, BigInteger amount)
        {
            var function = GetFunctionTransfer();
            object[] functionInput = { to.Substring(2), amount };
            var receipt = await BCComm.EvaluateOnBC(web3, from, function, functionInput);
            return receipt.Status.Value == 1;
        }
    }
}
