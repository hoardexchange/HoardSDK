using Hoard.BC.Contracts;
using Hoard.BC.Plasma;
using Hoard.Interfaces;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlasmaCore;
using PlasmaCore.RPC.OutputData;
using PlasmaCore.UTXO;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Hoard.BC
{
    /// <summary>
    /// Utility class for child chain (plasma) communication
    /// </summary>
    public class PlasmaComm : IBCComm
    {
        /// <summary>
        /// Zero address (160bits)
        /// </summary>
        public static string ZERO_ADDRESS = "0x0000000000000000000000000000000000000000";

        private BCComm bcComm = null;

        private PlasmaAPIService plasmaApiService = null;

        /// <summary>
        /// Creates PlasmaComm object.
        /// </summary>
        /// <param name="_bcComm">Ethereum blockchain communication</param>
        /// <param name="childChainClient"></param>
        /// <param name="watcherClient"></param>
        public PlasmaComm(BCComm _bcComm, PlasmaCore.RPC.IClient childChainClient, PlasmaCore.RPC.IClient watcherClient)
        {
            bcComm = _bcComm;
            plasmaApiService = new PlasmaAPIService(childChainClient, watcherClient);
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
        /// Returns UTXOs of given account and currency
        /// </summary>
        /// <param name="account">account to query</param>
        /// <param name="currency">currency to query</param>
        /// <returns></returns>
        public async Task<UTXOData[]> GetUtxos(HoardID account, string currency)
        {
            var utxos = await plasmaApiService.GetUtxos(account);
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

        private async Task<string> SendRequestPost(RestClient client, string method, object data)
        {
            var request = new RestRequest(method, Method.POST);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
            request.AddParameter("application/json", data.ToString(), ParameterType.RequestBody);
            var response = await client.ExecuteTaskAsync(request).ConfigureAwait(false); ;

            return response.Content;
        }
    }
}
