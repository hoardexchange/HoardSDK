using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
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
    public class HoardTokenContract
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

        public async Task<bool> Transfer(string from, string to, ulong amount)
        {
            var function = GetFunctionTransfer();
            var gas = await function.EstimateGasAsync(
                from,
                new Nethereum.Hex.HexTypes.HexBigInteger(3000000),
                new Nethereum.Hex.HexTypes.HexBigInteger(0),
                to,
                amount);

            gas = new Nethereum.Hex.HexTypes.HexBigInteger(gas.Value * 2);
            var receipt = await function.SendTransactionAndWaitForReceiptAsync(
                from,
                gas,
                new Nethereum.Hex.HexTypes.HexBigInteger(0),
                null,
                to,
                amount);
            return receipt.Status.Value == 1;
        }
    }
}
