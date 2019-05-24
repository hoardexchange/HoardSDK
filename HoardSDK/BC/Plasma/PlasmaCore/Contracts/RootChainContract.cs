using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using PlasmaCore.RPC.OutputData;
using System.Numerics;
using System.Threading.Tasks;

namespace Plasma.RootChain.Contracts
{
    public class RootChainContract
    {
        public static readonly BigInteger STANDARD_EXIT_BOND = new BigInteger(31415926535);

        public string Address { get { return contract.Address; } }

        private readonly Web3 web3;
        private Contract contract;
        private string abi;

        public RootChainContract(Web3 web3, string address, string abiVersion = null)
        {
            this.abi = RootChainABI.GetRootChainABI(abiVersion);
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(abi, address);
        }

        public async Task<Nethereum.Signer.Transaction> StartStandardExit(Web3 web, string address, ExitData exitData, BigInteger? exitBond = null)
        {
            var function = GetFunctionStartStandardExit();
            return await ContractHelper.CreateTransaction(web, address,
                exitBond.HasValue ? exitBond.Value : STANDARD_EXIT_BOND,
                function,
                exitData.Position.ToHex(true),
                exitData.TxBytes.HexToByteArray(),
                exitData.Proof.HexToByteArray());
        }

        public async Task<Nethereum.Signer.Transaction> ProcessExits(Web3 web, string address, string currency, BigInteger topUtxoPosition, BigInteger exitsToProcess)
        {
            var function = GetFunctionProcessExits();
            return await ContractHelper.CreateTransaction(web, address,
                function,
                currency.HexToByteArray(),
                topUtxoPosition,
                exitsToProcess);
        }

        public async Task<Nethereum.Signer.Transaction> Deposit(Web3 web, string address, byte[] depositTx, BigInteger amount)
        {
            var function = GetFunctionDeposit();
            return await ContractHelper.CreateTransaction(web, address,
                amount,
                function,
                depositTx);
        }

        private Function GetFunctionStartStandardExit()
        {
            return contract.GetFunction("startStandardExit");
        }

        private Function GetFunctionProcessExits()
        {
            return contract.GetFunction("processExits");
        }

        private Function GetFunctionDeposit()
        {
            return contract.GetFunction("deposit");
        }
    }
}
