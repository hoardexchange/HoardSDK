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
    public class HoardTokenContract
    {
        public const string ABI = HoardABIConfig.ERC721TokenMockABI;

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

        public Task<BigInteger> GetBalanceOf(string address)
        {
            var function = GetFunctionBalanceOf();
            return function.CallAsync<BigInteger>(address);
        }
    }
}
