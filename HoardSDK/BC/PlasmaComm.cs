using Hoard.BC.Contracts;
using Hoard.BC.Plasma;
using Hoard.Interfaces;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        private RestClient childChainClient = null;
        private RestClient watcherClient = null;

        private BCComm bcComm = null;

        /// <summary>
        /// Creates PlasmaComm object.
        /// </summary>
        /// <param name="_bcComm">Ethereum blockchain communication</param>
        /// <param name="childChainUrl">childchain rest client</param>
        /// <param name="watcherUrl">watcher rest client</param>
        public PlasmaComm(BCComm _bcComm, string childChainUrl, string watcherUrl)
        {
            bcComm = _bcComm;

            if (Uri.IsWellFormedUriString(childChainUrl, UriKind.Absolute))
            {
                childChainClient = new RestClient(childChainUrl);
                childChainClient.AutomaticDecompression = false;

                //setup a cookie container for automatic cookies handling
                childChainClient.CookieContainer = new System.Net.CookieContainer();
            }

            if (Uri.IsWellFormedUriString(watcherUrl, UriKind.Absolute))
            {
                watcherClient = new RestClient(watcherUrl);
                watcherClient.AutomaticDecompression = false;

                //setup a cookie container for automatic cookies handling
                watcherClient.CookieContainer = new System.Net.CookieContainer();
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
        public async Task<List<TokenData>> GetTokensData(HoardID account)
        {
            var data = new JObject();
            data.Add("address", account.ToString().EnsureHexPrefix());

            var responseString = await SendRequestPost(watcherClient, "account.get_balance", data);

            var responseJson = JObject.Parse(responseString);
            if (IsResponseSuccess(responseJson))
            {
                var responseData = GetResponseData(responseString);
                return JsonConvert.DeserializeObject<List<TokenData>>(responseData);
            }

            Trace.Fail(string.Format("Could not get tokens data! {0}", responseJson.Value<string>("description")));
            return new List<TokenData>();
        }

        /// <summary>
        /// Returns tokens data (balance) of given account and currency
        /// </summary>
        /// <param name="account">account to query</param>
        /// <param name="currency">currency to query</param>
        /// <returns></returns>
        public async Task<List<TokenData>> GetTokensData(HoardID account, string currency)
        {
            var data = new JObject();
            data.Add("address", account.ToString().EnsureHexPrefix());
            data.Add("currency", currency.EnsureHexPrefix());

            var responseString = await SendRequestPost(watcherClient, "account.get_balance", data);

            var responseJson = JObject.Parse(responseString);
            if (IsResponseSuccess(responseJson))
            {
                var tokenData = JsonConvert.DeserializeObject<List<TokenData>>(GetResponseData(responseString));

                //TODO: plasma api doesn't support request with currency filter
                if (tokenData.Exists(x => x.Currency.ToLower() == currency.ToLower()))
                {
                    return tokenData.FindAll(x => x.Currency.ToLower() == currency.ToLower());
                }
            }

            return new List<TokenData>();
        }

        /// <summary>
        /// Sumbits signed transaction to child chain
        /// </summary>
        /// <param name="signedTransaction">RLP encoded signed transaction</param>
        /// <returns></returns>
        public async Task<bool> SubmitTransaction(string signedTransaction)
        {
            var data = new JObject();
            data.Add("transaction", signedTransaction.EnsureHexPrefix());

            var responseString = await SendRequestPost(childChainClient, "transaction.submit", data);

            var responseJson = JObject.Parse(responseString);
            if (IsResponseSuccess(responseJson))
            {
                var responseData = GetResponseData(responseString);
                var receipt = JsonConvert.DeserializeObject<TransactionReceipt>(responseData);

                TransactionData transaction = null;
                transaction = await GetTransaction(receipt.TxHash);
                while (transaction == null)
                {
                    Thread.Sleep(1000);
                    transaction = await GetTransaction(receipt.TxHash);
                }

                return true;
            }

            Trace.Fail(string.Format("Could not submit transaction! {0}", responseJson.Value<string>("description")));
            return false;
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
            var balances = await GetTokensData(account);
            var utxoData = balances.FirstOrDefault(x => x.Currency.RemoveHexPrefix() == currency.RemoveHexPrefix());

            if (utxoData != null)
                return utxoData.Amount;
            return 0;
        }

        /// <summary>
        /// Returns UTXOs of given account and currency
        /// </summary>
        /// <param name="account">account to query</param>
        /// <param name="currency">currency to query</param>
        /// <returns></returns>
        public async Task<List<UTXOData>> GetUtxos(HoardID account, string currency)
        {
            var data = new JObject();
            data.Add("address", account.ToString().EnsureHexPrefix());

            var responseString = await SendRequestPost(watcherClient, "account.get_utxos", data);

            var responseJson = JObject.Parse(responseString);
            if (IsResponseSuccess(responseJson))
            {
                var result = new List<UTXOData>();

                var responseData = GetResponseData(responseString);
                var utxos = JsonConvert.DeserializeObject<List<UTXOData>>(responseData);

                foreach (var utxo in utxos)
                {
                    if (utxo.Currency == currency)
                    {
                        result.Add(utxo);
                    }
                }

                return result;
            }

            Trace.Fail(string.Format("Could not get utxos! {0}", responseJson.Value<string>("description")));
            return new List<UTXOData>();
        }

        /// <summary>
        /// Returns transaction data of given transaction hash
        /// </summary>
        /// <param name="txId">transaction hash to query</param>
        /// <returns></returns>
        protected async Task<TransactionData> GetTransaction(HexBigInteger txId)
        {
            var data = new JObject();
            data.Add("id", txId.HexValue.EnsureHexPrefix());

            var responseString = await SendRequestPost(watcherClient, "transaction.get", data);

            var responseJson = JObject.Parse(responseString);
            if (IsResponseSuccess(responseJson))
            {
                return JsonConvert.DeserializeObject<TransactionData>(GetResponseData(responseString));
            }

            return null;
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
                return (GameItemAdapter)Activator.CreateInstance(typeof(ERC223GameItemAdapter), this, game, contract);
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

        private bool IsResponseSuccess(JObject responseJson)
        {
            if (responseJson.ContainsKey("success"))
            {
                JToken success;
                responseJson.TryGetValue("success", out success);
                return (success.Value<bool>() == true);
            }
            return false;
        }

        private string GetResponseData(string responseString)
        {
            var result = JObject.Parse(responseString);
            if (result.ContainsKey("data"))
            {
                return result.GetValue("data").ToString();
            }
            return "";
        }
    }
}
