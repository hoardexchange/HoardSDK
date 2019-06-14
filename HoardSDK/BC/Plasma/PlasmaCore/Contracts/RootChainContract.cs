using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using PlasmaCore.RPC.OutputData;
using System;
using System.Numerics;
using System.Threading.Tasks;

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
        public RootChainContract(Web3 web3, string address, RootChainVersion abiVersion)
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

        /// <summary>
        /// Gets standard exit id for given output id
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="outputId">output id</param>
        /// <returns></returns>
        public async Task<BigInteger> GetStandardExitId(Web3 web3, BigInteger outputId)
        {
            var function = GetFunctionGetStandardExitId();
            return await function.CallAsync<BigInteger>(outputId);
        }

        /// <summary>
        /// Given an output ID, determines when it's exitable
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="outputId">output id</param>
        /// <returns></returns>
        public async Task<ulong> GetExitableTimestamp(Web3 web3, BigInteger outputId)
        {
            var function = GetFunctionGetExitableTimestamp();
            return await function.CallAsync<ulong>(outputId);
        }

        /// <summary>
        /// Checks if timestamp is exitable now
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="exitableTimestamp">request timestamp</param>
        /// <returns></returns>
        public async Task<bool> IsMature(Web3 web3, ulong exitableTimestamp)
        {
            var function = GetFunctionIsMature();
            return await function.CallAsync<bool>(exitableTimestamp);
        }

        /// <summary>
        /// Gets exits if given id
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="exitId">exit id</param>
        /// <returns></returns>
        public async Task<Tuple<string, string, BigInteger>> GetExit(Web3 web3, BigInteger exitId)
        {
            var function = GetFunctionGetExitableTimestamp();
            return await function.CallAsync<Tuple<string, string, BigInteger>>(exitId);
        }

        /// <summary>
        /// Adds token to plasma network
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="address">sender address</param>
        /// <param name="tokenAddress">token address</param>
        /// <returns></returns>
        public async Task<Nethereum.Signer.Transaction> AddToken(Web3 web3, string address, string tokenAddress)
        {
            var function = GetFunctionAddToken();
            return await ContractHelper.CreateTransaction(web3, address,
                function,
                tokenAddress);
        }

        /// <summary>
        /// Checks if token was added to plasma network
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="tokenAddress">token address</param>
        /// <returns></returns>
        public async Task<bool> HasToken(Web3 web3, string tokenAddress)
        {
            var function = GetFunctionHasToken();
            return await function.CallAsync<bool>(tokenAddress);
        }

        /// <summary>
        /// Challenges a standard exit
        /// </summary>
        /// <param name="web3">web3 interface</param>
        /// <param name="address">sender address</param>
        /// <param name="challengeData">challenge data</param>
        /// <returns></returns>
        public async Task<Nethereum.Signer.Transaction> ChallengeStandardExit(Web3 web3, string address, ChallengeData challengeData)
        {
            var function = GetFunctionChallengeStandardExit();
            return await ContractHelper.CreateTransaction(web3, address,
                function,
                challengeData.ExitId,
                challengeData.TxBytes.HexToByteArray(),
                challengeData.InputIndex,
                challengeData.Signature.HexToByteArray());
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

        private Function GetFunctionGetStandardExitId()
        {
            return contract.GetFunction("getStandardExitId");
        }

        private Function GetFunctionGetExitableTimestamp()
        {
            return contract.GetFunction("getExitableTimestamp");
        }

        private Function GetFunctionIsMature()
        {
            return contract.GetFunction("isMature");
        }

        private Function GetFunctionExits()
        {
            return contract.GetFunction("exits");
        }

        private Function GetFunctionAddToken()
        {
            return contract.GetFunction("addToken");
        }

        private Function GetFunctionHasToken()
        {
            return contract.GetFunction("hasToken");
        }

        private Function GetFunctionChallengeStandardExit()
        {
            return contract.GetFunction("challengeStandardExit");
        }
    }
}
