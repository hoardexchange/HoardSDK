using Hoard.BC.Plasma;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.JsonRpc.Client;
using Nethereum.RLP;
using Nethereum.Signer;
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
    public class PlasmaComm : BCComm
    {
        private static string ETH_CURRENCY_ADDRESS = "0x0000000000000000000000000000000000000000";
        private static string NULL_SIGNATURE = "0x0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000";
        private static BigInteger U256_MAX_VALUE = BigInteger.Pow(2, 256);
        private static UInt16 MAX_INPUTS = 2;
        private static UInt16 MAX_OUTPUTS = 2;

        private RestClient childChainClient = null;
        private RestClient watcherClient = null;

        /// <summary>
        /// Creates PlasmaComm object.
        /// </summary>
        /// <param name="ethClient">JsonRpc client implementation</param>
        /// <param name="childChainUrl">Childchain rest client</param>
        /// <param name="watcherUrl">Watcher rest client</param>
        /// <param name="gameCenterContract">game center contract address</param>
        public PlasmaComm(IClient ethClient, string childChainUrl, string watcherUrl, string gameCenterContract)
            : base(ethClient, gameCenterContract)
        {
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

        //Connect

        public override async Task<BigInteger> GetBalance(HoardID account)
        {
            return await GetBalance(account, ETH_CURRENCY_ADDRESS);
        }

        public override async Task<BigInteger> GetHRDBalance(HoardID account)
        {
            var hrdCurrencyAddress = await GetHRDAddress();
            return await GetBalance(account, hrdCurrencyAddress);
        }

        public override async Task<bool> TransferHRD(AccountInfo from, string toAddress, BigInteger amount)
        {
            //TODO do we really use that function anywhere?
            return false;
        }

        public async Task<bool> Transfer(AccountInfo fromAccount, string toAddress, string currencySymbol, BigInteger amount)
        {
            Debug.Assert(fromAccount != null);

            var inputUtxos = await GetInputUtxos(fromAccount.ID, currencySymbol, amount);
            if (inputUtxos != null)
            {
                var encodedTransaction = await CreateTransaction(inputUtxos, fromAccount, toAddress, amount);

                var data = JObject.Parse(string.Format("{{ \"transaction\" : \"{0}\" }}", encodedTransaction));
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
            }

            //TODO no sufficient funds
            throw new NotImplementedException();
        }

        protected async Task<BigInteger> GetBalance(HoardID account, string currencySymbol)
        {
            var data = JObject.Parse(string.Format("{{ \"address\" : \"{0}\" }}", account.ToString()));
            var responseString = await SendRequestPost(watcherClient, "account.get_balance", data);

            if (IsResponseSuccess(responseString))
            {
                var balances = JsonConvert.DeserializeObject<List<BalanceData>>(GetResponseData(responseString));
                foreach (var balance in balances)
                {
                    if (balance.Currency == currencySymbol.RemoveHexPrefix())
                    {
                        return balance.Amount;
                    }
                }

                return 0;
            }

            //TODO
            throw new NotImplementedException();
        }

        protected async Task<List<UTXOData>> GetInputUtxos(HoardID account, string currencySymbol, BigInteger amount)
        {
            var utxos = await GetUtxos(account, currencySymbol);
            utxos.RemoveAll(x => x.Amount == 0);
            utxos.Insert(0, new UTXOData(new BigInteger(0)));

            var sortedUtxos = utxos.OrderBy(x => x.Amount).ToArray();

            var sortedUtxosCount = (UInt32)sortedUtxos.Count();
            if (sortedUtxosCount > 1)
            {
                // find single utxo or pair of utxos closest to given amount
                if (sortedUtxos[sortedUtxosCount - 1].Amount + sortedUtxos[sortedUtxosCount - 2].Amount >= amount)
                {
                    UInt32 resultIdxL = 0, resultIdxR = 0;
                    UInt32 idxL = 0, idxR = sortedUtxosCount;
                    var diff = U256_MAX_VALUE;

                    while (idxR > idxL)
                    {
                        var utxoSum = sortedUtxos[idxL].Amount + sortedUtxos[idxR].Amount;
                        if (utxoSum >= amount && utxoSum - amount < diff)
                        {
                            resultIdxL = idxL;
                            resultIdxR = idxR;
                            diff = utxoSum - amount;
                        }

                        if (utxoSum > amount)
                        {
                            idxR--;
                        }
                        else
                        {
                            idxL++;
                        }
                    }

                    Debug.Assert(sortedUtxos[resultIdxL].Amount + sortedUtxos[resultIdxR].Amount >= amount);

                    if (resultIdxL > 0)
                        return new List<UTXOData>() { sortedUtxos[resultIdxL], sortedUtxos[resultIdxR] };
                    return new List<UTXOData>() { sortedUtxos[resultIdxR] };

                }
                else
                {
                    var sum = new BigInteger(0);
                    utxos.ForEach(x => sum += x.Amount);

                    if (sum >= amount)
                    {
                        //TODO merge utxos and check if it is possible to find pair of utxo that is greater or equal than given amount
                        throw new NotImplementedException();
                    }
                }
            }

            // no sufficient funds
            return null;
        }

        protected async Task<List<UTXOData>> GetUtxos(HoardID account, string currencySymbol)
        {
            var data = JObject.Parse(string.Format("{{ \"address\" : \"{0}\" }}", account.ToString()));
            var responseString = await SendRequestPost(watcherClient, "account.get_utxos", data);

            if (IsResponseSuccess(responseString))
            {
                var result = new List<UTXOData>();
                var utxos = JsonConvert.DeserializeObject<List<UTXOData>>(GetResponseData(responseString));

                return utxos.Where(x => x.Currency == currencySymbol).ToList();
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

        protected async Task<string> CreateTransaction(List<UTXOData> inputUtxos, AccountInfo fromAccount, string toAddress, BigInteger amount)
        {
            Debug.Assert(inputUtxos.Count <= 2);

            // create transaction data
            var txData = new List<byte[]>();
            for(UInt16 i = 0; i < MAX_INPUTS; ++i)
            {
                if (i < inputUtxos.Count())
                {
                    // cannot mix currencies
                    Debug.Assert(inputUtxos[0].Currency == inputUtxos[i].Currency);

                    txData.Add(inputUtxos[i].BlkNum.ToBytesForRLPEncoding());
                    txData.Add(inputUtxos[i].TxIndex.ToBytesForRLPEncoding());
                    txData.Add(inputUtxos[i].OIndex.ToBytesForRLPEncoding());
                }
                else
                {
                    txData.Add(BigInteger.Zero.ToBytesForRLPEncoding());
                    txData.Add(BigInteger.Zero.ToBytesForRLPEncoding());
                    txData.Add(BigInteger.Zero.ToBytesForRLPEncoding());
                }
            }

            txData.Add(inputUtxos[0].Currency.HexToByteArray());

            txData.Add(toAddress.HexToByteArray());
            txData.Add(amount.ToBytesForRLPEncoding());

            var sum = new BigInteger(0);
            inputUtxos.ForEach(x => sum += x.Amount);
            if (sum > amount)
            {
                txData.Add(fromAccount.ID.ToHexByteArray());
                txData.Add((sum - amount).ToBytesForRLPEncoding());
            }
            else
            {
                txData.Add("0x0000000000000000000000000000000000000000".HexToByteArray());
                txData.Add(BigInteger.Zero.ToBytesForRLPEncoding());
            }

            // sign transaction
            var tx = new RLPSigner(txData.ToArray());
            var signature = await fromAccount.SignTransaction(tx.GetRLPEncodedRaw());

            txData.Add(signature.HexToByteArray());
            txData.Add(NULL_SIGNATURE.HexToByteArray());

            tx = new RLPSigner(txData.ToArray());

            return tx.GetRLPEncodedRaw().ToHex().ToLower(); ;
        }

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
    }
}
