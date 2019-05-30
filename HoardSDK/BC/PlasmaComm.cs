using Hoard.BC.Contracts;
using Hoard.BC.Plasma;
using Hoard.Interfaces;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Plasma.RootChain.Contracts;
using PlasmaCore;
using PlasmaCore.RPC.OutputData;
using PlasmaCore.UTXO;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Hoard.BC
{
    /// <summary>
    /// Utility class for Plasma communication
    /// </summary>
    public class PlasmaComm : IBCComm
    {
        /// <summary>
        /// Zero address (160bits) / default ethereum currency
        /// </summary>
        public static string ZERO_ADDRESS = "0x0000000000000000000000000000000000000000";
        
        private Web3 web3 = null;

        private BCComm bcComm = null;

        private PlasmaAPIService plasmaApiService = null;

        private RootChainContract rootChainContract = null;

        /// <summary>
        /// Creates PlasmaComm object.
        /// </summary>
        /// <param name="ethClient">Ethereum jsonRpc client implementation</param>
        /// <param name="gameCenterContract">game center contract address</param>
        /// <param name="watcherClient">watcher client</param>
        /// <param name="childChainClient">child chain client</param>
        /// <param name="rootChainAddress">root chain address</param>
        /// <param name="rootChainAbiVersion">root chain abi version (optional)</param>
        public PlasmaComm(Nethereum.JsonRpc.Client.IClient ethClient, 
                        string gameCenterContract, 
                        PlasmaCore.RPC.IClient watcherClient, 
                        PlasmaCore.RPC.IClient childChainClient,
                        string rootChainAddress,
                        string rootChainAbiVersion = null)
        {
            web3 = new Web3(ethClient);
            bcComm = new BCComm(ethClient, gameCenterContract);

            plasmaApiService = new PlasmaAPIService(watcherClient, childChainClient);
            if (rootChainAddress != null)
            {
                rootChainContract = new RootChainContract(web3, rootChainAddress, rootChainAbiVersion);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<Tuple<bool, string>> Connect()
        {
            return await bcComm.Connect();
        }

        /// <inheritdoc/>
        public async Task<BigInteger> GetBalance(HoardID account)
        {
            return await GetBalance(account, ZERO_ADDRESS);
        }

        /// <inheritdoc/>
        public async Task<BigInteger> GetHRDBalance(HoardID account)
        {
            return await GetBalance(account, await bcComm.GetHRDAddress());
        }

        /// <inheritdoc/>
        public async Task<Result> RegisterHoardGame(GameID game)
        {
            return await bcComm.RegisterHoardGame(game);
        }

        /// <inheritdoc/>
        public void UnregisterHoardGame(GameID game)
        {
            bcComm.UnregisterHoardGame(game);
        }

        /// <inheritdoc/>
        public GameID[] GetRegisteredHoardGames()
        {
            return bcComm.GetRegisteredHoardGames();
        }

        /// <inheritdoc/>
        public async Task<GameID[]> GetHoardGames()
        {
            return await bcComm.GetHoardGames();
        }

        /// <inheritdoc/>
        public async Task<bool> GetGameExists(BigInteger gameID)
        {
            return await bcComm.GetGameExists(gameID);
        }

        /// <inheritdoc/>
        public async Task<string> GetHoardExchangeContractAddress()
        {
            return await bcComm.GetHoardExchangeContractAddress();
        }

        /// <inheritdoc/>
        public async Task<UInt64> GetHoardGameCount()
        {
            return await bcComm.GetHoardGameCount();
        }

        /// <summary>
        /// Returns tokens data (balance) of given account
        /// </summary>
        /// <param name="account">account to query</param>
        /// <returns></returns>
        public async Task<BalanceData[]> GetBalanceData(HoardID account)
        {
            return await plasmaApiService.GetBalance(account);
        }

        /// <summary>
        /// Returns tokens data (balance) of given account and currency
        /// </summary>
        /// <param name="account">account to query</param>
        /// <param name="currency">currency to query</param>
        /// <returns></returns>
        public async Task<BalanceData[]> GetBalanceData(HoardID account, string currency)
        {
            var balance = await plasmaApiService.GetBalance(account);
            return balance.Where(x => x.Currency.RemoveHexPrefix().ToLower() == currency.RemoveHexPrefix().ToLower()).ToArray();
        }

        /// <summary>
        /// Sumbits signed transaction to child chain
        /// </summary>
        /// <param name="signedTransaction">RLP encoded signed transaction</param>
        /// <returns></returns>
        public async Task<TransactionDetails> SubmitTransaction(string signedTransaction)
        {
            var receipt = await plasmaApiService.SubmitTransaction(signedTransaction);
            TransactionDetails transaction = null;

            // timeout
            do
            {
                Thread.Sleep(1000);
                transaction = await plasmaApiService.GetTransaction(receipt.TxHash.HexValue);
            } while (transaction == null);

            return transaction;
        }

        /// <summary>
        /// Returns current token state (ERC721)
        /// </summary>
        /// <param name="currency">currency to query</param>
        /// <param name="tokenId">id to query</param>
        /// <returns></returns>
        public async Task<byte[]> GetTokenState(string currency, BigInteger tokenId)
        {
            //TODO not implemented in plasma
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns balance of given account and currency
        /// </summary>
        /// <param name="account">account to query</param>
        /// <param name="currency">currency to query</param>
        /// <returns></returns>
        protected async Task<BigInteger> GetBalance(HoardID account, string currency)
        {
            var balances = await GetBalanceData(account);
            var utxoData = balances.FirstOrDefault(x => x.Currency.RemoveHexPrefix().ToLower() == currency.RemoveHexPrefix().ToLower());

            if (utxoData != null)
                return (utxoData as FCBalanceData).Amount;
            return 0;
        }

        /// <summary>
        /// Returns all UTXOs of given account
        /// </summary>
        /// <param name="account">account to query</param>
        /// <returns></returns>
        public async Task<UTXOData[]> GetUtxos(HoardID account)
        {
            return await plasmaApiService.GetUtxos(account);
        }

        /// <summary>
        /// Returns UTXOs of given account in given currency
        /// </summary>
        /// <param name="account">account to query</param>
        /// <param name="currency">currency to query</param>
        /// <returns></returns>
        public async Task<UTXOData[]> GetUtxos(HoardID account, string currency)
        {
            var utxos = await GetUtxos(account);
            var result = new List<UTXOData>();
            foreach (var utxo in utxos)
            {
                if (utxo.Currency.RemoveHexPrefix().ToLower() == currency.RemoveHexPrefix().ToLower())
                {
                    result.Add(utxo);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Returns UTXO of given account, currency and amount
        /// </summary>
        /// <param name="account">account to query</param>
        /// <param name="currency">currency to query</param>
        /// <param name="amount">amount to query</param>
        /// <returns></returns>
        public async Task<UTXOData> GetUtxo(HoardID account, string currency, BigInteger amount)
        {
            var utxos = await GetUtxos(account);
            foreach (var utxo in utxos)
            {
                if (utxo is FCUTXOData &&
                    utxo.Currency.RemoveHexPrefix().ToLower() == currency.RemoveHexPrefix().ToLower() &&
                    (utxo as FCUTXOData).Amount == amount)
                {
                    return utxo;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns transaction data of given transaction hash
        /// </summary>
        /// <param name="txId">transaction hash to query</param>
        /// <returns></returns>
        protected async Task<TransactionDetails> GetTransaction(HexBigInteger txHash)
        {
            return await plasmaApiService.GetTransaction(txHash.HexValue);
        }

        /// <summary>
        /// Creates game item adapter for given game and game item contract
        /// </summary>
        /// <param name="game">game id</param>
        /// <param name="contract">game item contract</param>
        /// <returns></returns>
        public GameItemAdapter GetGameItemAdater(GameID game, GameItemContract contract)
        {
            if (contract is ERC223GameItemContract)
                return (GameItemAdapter)Activator.CreateInstance(typeof(ERC20GameItemAdapter), this, game, contract);
            else if (contract is ERC721GameItemContract)
                return (GameItemAdapter)Activator.CreateInstance(typeof(ERC721GameItemAdapter), this, game, contract);

            throw new NotSupportedException();
        }

        /// <summary>
        /// Helper function to get contract of a prticular type
        /// </summary>
        /// <param name="contractType">type of contract</param>
        /// <param name="contractAddress">address of the contract</param>
        /// <returns></returns>
        public object GetContract(Type contractType, string contractAddress)
        {
            return bcComm.GetContract(contractType, contractAddress);
        }

        /// <summary>
        /// Returns GameItem contract for given game and of given type
        /// </summary>
        /// <param name="game"></param>
        /// <param name="contractAddress"></param>
        /// <param name="contractType"></param>
        /// <param name="abi">[optional] creates contract with a particular abi</param>
        /// <returns></returns>
        public GameItemContract GetGameItemContract(GameID game, string contractAddress, Type contractType, string abi = "")
        {
            return bcComm.GetGameItemContract(game, contractAddress, contractType, abi);
        }

        /// <summary>
        /// Retrieves all GameItem contract addresses registered for a particular game
        /// </summary>
        /// <param name="game">game to query</param>
        /// <returns></returns>
        public async Task<string[]> GetGameItemContracts(GameID game)
        {
            return await bcComm.GetGameItemContracts(game);
        }

        /// <summary>
        /// Starts standard exit on the root chain
        /// </summary>
        /// <param name="profileFrom"></param>
        /// <param name="utxoData"></param>
        /// <param name="exitBond"></param>
        /// <param name="tokenSource">cancellation token source</param>
        /// <returns></returns>
        public async Task<Nethereum.RPC.Eth.DTOs.TransactionReceipt> StartStandardExit(Profile profileFrom, UTXOData utxoData, BigInteger? exitBond = null, CancellationTokenSource tokenSource = null)
        {
            if (rootChainContract != null)
            {
                ExitData exitData = await plasmaApiService.GetExitData(utxoData.Position);
                var transaction = await rootChainContract.StartStandardExit(web3, profileFrom.ID, exitData, exitBond);
                string signedTransaction = await SignTransaction(profileFrom, transaction);
                return await SubmitTransactionOnRootChain(web3, signedTransaction, tokenSource);
            }
            return null;
        }

        /// <summary>
        /// Processes standard exit on the root chain
        /// </summary>
        /// <param name="profileFrom"></param>
        /// <param name="currency">transaction currency</param>
        /// <param name="topUtxoPosition">starting index of exit</param>
        /// <param name="exitsToProcess">number exits to process</param>
        /// <param name="tokenSource">cancellation token source</param>
        /// <returns></returns>
        public async Task<Nethereum.RPC.Eth.DTOs.TransactionReceipt> ProcessExits(Profile profileFrom, string currency, BigInteger topUtxoPosition, BigInteger exitsToProcess, CancellationTokenSource tokenSource = null)
        {
            if (rootChainContract != null)
            {
                var transaction = await rootChainContract.ProcessExits(web3, profileFrom.ID, currency, topUtxoPosition, exitsToProcess);
                string signedTransaction = await SignTransaction(profileFrom, transaction);
                return await SubmitTransactionOnRootChain(web3, signedTransaction, tokenSource);
            }
            return null;
        }

        /// <summary>
        /// Deposits given amount of ether to the child chain
        /// </summary>
        /// <param name="profileFrom">transaction sender</param>
        /// <param name="amount">transaction amount</param>
        /// <param name="tokenSource">cancellation token source</param>
        /// <returns></returns>
        public async Task<Nethereum.RPC.Eth.DTOs.TransactionReceipt> Deposit(Profile profileFrom, BigInteger amount, CancellationTokenSource tokenSource = null)
        {
            if (rootChainContract != null)
            {
                var depositPlasmaTx = new PlasmaCore.Transactions.Transaction();
                depositPlasmaTx.AddOutput(profileFrom.ID, ZERO_ADDRESS, amount);

                var depositTx = await rootChainContract.Deposit(web3, profileFrom.ID, depositPlasmaTx.GetRLPEncodedRaw(), amount);
                string signedDepositTx = await SignTransaction(profileFrom, depositTx);
                return await SubmitTransactionOnRootChain(web3, signedDepositTx, tokenSource);
            }
            return null;
        }

        /// <summary>
        /// Deposits given amount of ERC20 token to the child chain
        /// </summary>
        /// <param name="profileFrom">transaction sender</param>
        /// <param name="currency">transaction currency</param>
        /// <param name="amount">transaction amount</param>
        /// <param name="tokenSource">cancellation token source</param>
        /// <returns></returns>
        public async Task<Nethereum.RPC.Eth.DTOs.TransactionReceipt> Deposit(Profile profileFrom, string currency, BigInteger amount, CancellationTokenSource tokenSource = null)
        {
            if (rootChainContract != null)
            {
                var erc20Handler = web3.Eth.GetContractHandler(currency);
                var approveFunc = erc20Handler.GetFunction<Nethereum.StandardTokenEIP20.ContractDefinition.ApproveFunction>();

                var approveInput = new Nethereum.StandardTokenEIP20.ContractDefinition.ApproveFunction();
                approveInput.Spender = rootChainContract.Address;
                approveInput.AmountToSend = amount;

                var approveTx = await ContractHelper.CreateTransaction(web3, profileFrom.ID, BigInteger.Zero, approveFunc, approveInput);
                string signedApproveTx = await SignTransaction(profileFrom, approveTx);
                var receipt = await SubmitTransactionOnRootChain(web3, signedApproveTx, tokenSource);

                if (receipt.Status.Value == 1)
                {
                    var depositPlasmaTx = new PlasmaCore.Transactions.Transaction();
                    depositPlasmaTx.AddOutput(profileFrom.ID, currency, amount);

                    var depositTx = await rootChainContract.DepositToken(web3, profileFrom.ID, depositPlasmaTx.GetRLPEncodedRaw(), amount);
                    string signedDepositTx = await SignTransaction(profileFrom, depositTx);
                    return await SubmitTransactionOnRootChain(web3, signedDepositTx, tokenSource);
                }
                return receipt;
            }
            return null;
        }

        private async Task<string> SendRequestPost(RestClient client, string method, object data)
        {
            var request = new RestRequest(method, Method.POST);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
            request.AddParameter("application/json", data.ToString(), ParameterType.RequestBody);
            var response = await client.ExecuteTaskAsync(request).ConfigureAwait(false); ;

            return response.Content;
        }

        private static async Task<string> SignTransaction(Profile profile, Nethereum.Signer.Transaction transaction)
        {
            string signature = await profile.SignTransaction(transaction.GetRLPEncodedRaw());
            transaction.SetSignature(Nethereum.Signer.EthECDSASignatureFactory.ExtractECDSASignature(signature));
            return transaction.GetRLPEncoded().ToHex(true);
        }

        private static async Task<Nethereum.RPC.Eth.DTOs.TransactionReceipt> SubmitTransactionOnRootChain(Web3 web3, string signedTransaction, CancellationTokenSource tokenSource = null)
        {
            string txId = await web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction).ConfigureAwait(false);
            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
            while (receipt == null)
            {
                if (tokenSource != null && tokenSource.IsCancellationRequested)
                {
                    break;
                }
                Thread.Sleep(1000);
                receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txId);
            }
            return receipt;
        }
    }
}
