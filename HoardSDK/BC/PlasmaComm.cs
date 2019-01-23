using Hoard.BC.Contracts;
using Hoard.BC.Plasma;
using Hoard.Interfaces;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Hoard.BC
{
    // TODO Plasma comm should inherit from bccomm
    public class PlasmaComm : IBCComm
    {
        private static string ETH_CURRENCY_ADDRESS = "0x0000000000000000000000000000000000000000";
        private static UInt16 MAX_INPUTS = 2;
        private static UInt16 MAX_OUTPUTS = 2;

        private RestClient childChainClient = null;
        private RestClient watcherClient = null;

        private BCComm bcComm = null;

        /// <summary>
        /// Creates PlasmaComm object.
        /// </summary>
        /// <param name="bcComm"></param>
        /// <param name="childChainUrl">Childchain rest client</param>
        /// <param name="watcherUrl">Watcher rest client</param>
        /// <param name="gameCenterContract">game center contract address</param>
        public PlasmaComm(BCComm _bcComm, string childChainUrl, string watcherUrl)
        // : base(ethClient, gameCenterContract)
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

        /// <summary>
        /// Connects to blockchain using the JsonRpc client and performs a handshake
        /// </summary>
        /// <returns>a pair of [bool result, string return infromation] received from client</returns>
        public virtual async Task<Tuple<bool, string>> Connect()
        {
            return await bcComm.Connect();
        }

        public async Task<BigInteger> GetBalance(HoardID account)
        {
            return await GetBalance(account, ETH_CURRENCY_ADDRESS);
        }

        public async Task<BigInteger> GetHRDBalance(HoardID account)
        {
            return await GetBalance(account, await GetHRDAddress());
        }

        /// <summary>
        /// Retrieves HRD token address from game center
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetHRDAddress()
        {
            return await bcComm.GetHRDAddress();
        }

        public async Task<List<TokenData>> GetTokensData(HoardID account)
        {
            var data = JObject.Parse(string.Format("{{ \"address\" : \"{0}\" }}", account.ToString()));
            var responseString = await SendRequestPost(watcherClient, "account.get_balance", data);

            if (IsResponseSuccess(responseString))
            {
                return JsonConvert.DeserializeObject<List<TokenData>>(GetResponseData(responseString));
            }

            //TODO
            throw new NotImplementedException();
        }

        public async Task<List<TokenData>> GetTokensData(HoardID account, string currency)
        {
            var data = JObject.Parse(string.Format("{{ \"address\" : \"{0}\", \"currency\" : \"{1}\" }}", account.ToString(), currency));
            var responseString = await SendRequestPost(watcherClient, "account.get_balance", data);

            if (IsResponseSuccess(responseString))
            {
                return JsonConvert.DeserializeObject<List<TokenData>>(GetResponseData(responseString));
            }

            //TODO
            throw new NotImplementedException();
        }

        public async Task<bool> TransferHRD(AccountInfo from, string toAddress, BigInteger amount)
        {
            //TODO do we really use that function anywhere?
            return false;
        }

        public async Task<bool> SubmitTransaction(string signedTransaction)
        {
            var data = JObject.Parse(string.Format("{{ \"transaction\" : \"{0}\" }}", signedTransaction));
            var responseString = await SendRequestPost(childChainClient, "transaction.submit", data);

            if (IsResponseSuccess(responseString))
            {
                var receipt = JsonConvert.DeserializeObject<TransactionReceipt>(GetResponseData(responseString));

                TransactionData transaction = null;
                transaction = await GetTransaction(receipt.TxHash);
                while (transaction == null)
                {
                    Thread.Sleep(1000);
                    transaction = await GetTransaction(receipt.TxHash);
                }

                return true;
            }

            //TODO no sufficient funds
            throw new NotImplementedException();
        }

        public async Task<byte[]> GetTokenState(HoardID account, string currencySymbol, BigInteger tokenId)
        {
            //TODO not implemented in plasma
            throw new NotImplementedException();
        }

        protected async Task<BigInteger> GetBalance(HoardID account, string currencySymbol)
        {
            var balances = await GetTokensData(account);
            var utxoData = balances.FirstOrDefault(x => x.Currency == currencySymbol.RemoveHexPrefix());

            if (utxoData != null)
                return utxoData.Amount;
            return 0;
        }

        public async Task<List<UTXOData>> GetUtxos(HoardID account, string currency)
        {
            var data = JObject.Parse(string.Format("{{ \"address\" : \"{0}\" }}", account.ToString()));
            var responseString = await SendRequestPost(watcherClient, "account.get_utxos", data);

            if (IsResponseSuccess(responseString))
            {
                var result = new List<UTXOData>();

                var jsonUtxos = JsonConvert.DeserializeObject<List<string>>(GetResponseData(responseString));
                foreach (var jsonUtxo in jsonUtxos)
                {
                    var utxo = UTXODataFactory.Deserialize(jsonUtxo);
                    if (utxo.Currency == currency)
                    {
                        result.Add(utxo);
                    }
                }

                return result;
            }

            //TODO
            throw new NotImplementedException();
        }

        protected async Task<TransactionData> GetTransaction(BigInteger txId)
        {
            var data = JObject.Parse(string.Format("{{ \"id\" : \"{0}\" }}", txId.ToString("x")));
            var responseString = await SendRequestPost(watcherClient, "transaction.get", data);

            if (IsResponseSuccess(responseString))
            {
                return JsonConvert.DeserializeObject<TransactionData>(GetResponseData(responseString));
            }

            //TODO
            throw new NotImplementedException();
        }

        //protected async Task<List<byte[]>> CreateTransaction(List<UTXOData> inputUtxos, AccountInfo fromAccount, string toAddress, BigInteger amount)
        //{
        //    Debug.Assert(inputUtxos.Count <= 2);

        //     create transaction data
        //    var txData = new List<byte[]>();
        //    for(UInt16 i = 0; i < MAX_INPUTS; ++i)
        //    {
        //        if (i < inputUtxos.Count())
        //        {
        //             cannot mix currencies
        //            Debug.Assert(inputUtxos[0].Currency == inputUtxos[i].Currency);

        //            txData.Add(inputUtxos[i].BlkNum.ToBytesForRLPEncoding());
        //            txData.Add(inputUtxos[i].TxIndex.ToBytesForRLPEncoding());
        //            txData.Add(inputUtxos[i].OIndex.ToBytesForRLPEncoding());
        //        }
        //        else
        //        {
        //            txData.Add(BigInteger.Zero.ToBytesForRLPEncoding());
        //            txData.Add(BigInteger.Zero.ToBytesForRLPEncoding());
        //            txData.Add(BigInteger.Zero.ToBytesForRLPEncoding());
        //        }
        //    }

        //    txData.Add(inputUtxos[0].Currency.HexToByteArray());

        //    txData.Add(toAddress.HexToByteArray());
        //    txData.Add(amount.ToBytesForRLPEncoding());

        //    var sum = new BigInteger(0);
        //    inputUtxos.ForEach(x => sum += x.Amount);
        //    if (sum > amount)
        //    {
        //        txData.Add(fromAccount.ID.ToHexByteArray());
        //        txData.Add((sum - amount).ToBytesForRLPEncoding());
        //    }
        //    else
        //    {
        //        txData.Add("0x0000000000000000000000000000000000000000".HexToByteArray());
        //        txData.Add(BigInteger.Zero.ToBytesForRLPEncoding());
        //    }

        //    return txData;
        //}

        private async Task<string> SendRequestPost(RestClient client, string method, object data)
        {
            var request = new RestRequest(method, Method.POST);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
            request.AddJsonBody(data);

            var response = await client.ExecuteTaskAsync(request).ConfigureAwait(false); ;

            return response.Content;
        }

        private bool IsResponseSuccess(string responseString)
        {
            if (!string.IsNullOrEmpty(responseString))
            {
                var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);

                string success = "false";
                result.TryGetValue("success", out success);
                return (success == "true");
            }
            return false;
        }

        private string GetResponseData(string responseString)
        {
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseString);
            string data = "";
            result.TryGetValue("data", out data);
            return data;
        }

        public async Task<bool> RegisterHoardGame(GameID game)
        {
            return await bcComm.RegisterHoardGame(game);
        }

        public void UnregisterHoardGame(GameID game)
        {
            bcComm.UnregisterHoardGame(game);
        }

        public GameID[] GetRegisteredHoardGames()
        {
            return bcComm.GetRegisteredHoardGames();
        }

        public async Task<GameID[]> GetHoardGames()
        {
            return await bcComm.GetHoardGames();
        }

        public async Task<bool> GetGameExists(BigInteger gameID)
        {
            return await bcComm.GetGameExists(gameID);
        }

        public async Task<string> GetHoardExchangeContractAddress()
        {
            return await bcComm.GetHoardExchangeContractAddress();
        }

        public GameItemAdapter GetGameItemAdater(GameID game, GameItemContract contract)
        {
            if (contract is ERC223GameItemContract)
                return (GameItemAdapter)Activator.CreateInstance(typeof(ERC223GameItemAdapter), this, game, contract);
            else if (contract is ERC721GameItemContract)
                return (GameItemAdapter)Activator.CreateInstance(typeof(ERC721GameItemAdapter), this, game, contract);

            throw new NotSupportedException();
        }

        public GameItemContract GetGameItemContract(GameID game, string contractAddress, Type contractType, string abi = "")
        {
            return bcComm.GetGameItemContract(game, contractAddress, contractType, abi);
        }

        public async Task<string[]> GetGameItemContracts(GameID game)
        {
            return await GetGameItemContracts(game);
        }
    }
}
