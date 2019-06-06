using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using PlasmaCore.RPC.OutputData;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.ABI;

namespace Plasma.RootChain.Contracts
{
    /// <summary>
    /// Plasma root chain contract
    /// </summary>
    public class RootChainContract
    {
        private static readonly BigInteger STANDARD_EXIT_BOND = new BigInteger(31415926535);

        /// <summary>
        /// Address of this contract
        /// </summary>
        public string Address { get { return contract.Address; } }

        private readonly Web3 web3;
        private Contract contract;
        private string abi;

        /// <summary>
        /// Creates a new root chain contract
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="address">address of this contract (u160)</param>
        /// <param name="abiVersion">abi version of root chain contract</param>
        public RootChainContract(Web3 web3, string address, string abiVersion = null)
        {
            this.web3 = web3;
            abi = RootChainABI.GetRootChainABI(abiVersion);
            if (abi == null)
            {
                throw new ArgumentException("ABI version not found");
            }
            contract = web3.Eth.GetContract(abi, address);
        }

        /// <summary>
        /// Creates transaction of standard exit from child chain to root chain
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="address">sender address</param>
        /// <param name="exitData">exit data for a given utxo</param>
        /// <param name="exitBond">exit bond value (optional)</param>
        /// <returns></returns>
        public async Task<Nethereum.Signer.Transaction> StartStandardExit(Web3 web3, string address, ExitData exitData, BigInteger? exitBond = null)
        {
            var function = GetFunctionStartStandardExit();
            return await ContractHelper.CreateTransaction(web3, address,
                exitBond.HasValue ? exitBond.Value : STANDARD_EXIT_BOND,
                function,
                exitData.Position,
                exitData.TxBytes.HexToByteArray(),
                exitData.Proof.HexToByteArray());
        }

        /// <summary>
        /// Creates transaction of exit process to root chain
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="address">sender address</param>
        /// <param name="currency">transaction currency</param>
        /// <param name="topUtxoPosition">starting index of exit</param>
        /// <param name="exitsToProcess">number exits to process</param>
        /// <returns></returns>
        public async Task<Nethereum.Signer.Transaction> ProcessExits(Web3 web3, string address, string currency, BigInteger topUtxoPosition, BigInteger exitsToProcess)
        {
            var function = GetFunctionProcessExits();
            return await ContractHelper.CreateTransaction(web3, address,
                function,
                currency,
                topUtxoPosition,
                exitsToProcess);
        }

        /// <summary>
        /// Creates transaction of ETH deposit to root chain
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="address">sender address</param>
        /// <param name="depositTx">deposit transaction data</param>
        /// <param name="amount">amount of deposit</param>
        /// <returns></returns>
        public async Task<Nethereum.Signer.Transaction> Deposit(Web3 web3, string address, byte[] depositTx, BigInteger amount)
        {
            var function = GetFunctionDeposit();
            return await ContractHelper.CreateTransaction(web3, address,
                amount,
                function,
                depositTx);
        }


        /// <summary>
        /// Creates transaction of ERC20 deposit to rootchain (caller must be token owner)
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="address">sender address</param>
        /// <param name="depositTx">deposit transaction data</param>
        /// <returns></returns>
        public async Task<Nethereum.Signer.Transaction> DepositToken(Web3 web3, string address, byte[] depositTx)
        {
            var function = GetFunctionDepositFrom();
            return await ContractHelper.CreateTransaction(web3, address,
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

        private Function GetFunctionDepositFrom()
        {
            return contract.GetFunction("depositFrom");
        }
    }
}
