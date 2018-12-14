using Hoard.BC.Contracts;
using Hoard.Utils;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.GameItemProviders
{
    /// <summary>
    /// Default Hoard Platform GameItem Provider supports all GameItems complying to Hoard standards.
    /// </summary>
    public class HoardGameItemProvider : IGameItemProvider
    {
        public GameID Game { get; private set; }
        /// <summary>
        /// Secure provider used for validation but also in case server is down. Should be set to sth reliable like BCGameItemProvider.
        /// </summary>
        public IGameItemProvider SecureProvider { get; set; } = null;

        private RestClient Client = null;
        private string SessionKey = null;

        /// <summary>
        /// Cached list of all supported item types as returned from server
        /// </summary>
        private List<string> ItemTypes = null;

        public HoardGameItemProvider(GameID game)
        {
            Game = game;
        }

        public void Shutdown()
        {
        }

        private async Task<bool> ConnectToGameServer()
        {
            if (Uri.IsWellFormedUriString(Game.Url, UriKind.Absolute))
            {                
                Client = new RestClient(Game.Url);
                Client.AutomaticDecompression = false;
                //setup a cookie container for automatic cookies handling
                Client.CookieContainer = new System.Net.CookieContainer();

                //handshake

                //1. GET challenge token
                var request = new RestRequest("login/", Method.GET);
                request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
                var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false);

                if (response.ErrorException != null)
                    return false;
                
                string challengeToken = response.Content;
                challengeToken = challengeToken.Substring(2);

                var nonce = Eth.Utils.Mine(challengeToken, new BigInteger(1) << 496);
                var nonceHex = nonce.ToString("x");

                //generate new secure random key
                var ecKey = Nethereum.Signer.EthECKey.GenerateKey();

                var dataBytes = Encoding.ASCII.GetBytes(response.Content.Substring(2) + nonceHex);
                string sig = await KeyStoreAccountService.SignMessage(dataBytes,ecKey.GetPrivateKey()).ConfigureAwait(false);
                if (sig == null)
                    return false;

                var responseLogin = PostJson("login/", new
                {
                    token = response.Content,
                    nonce = "0x" + nonceHex,
                    address = ecKey.GetPublicAddress(),
                    signature = sig
                }).Result;

                if (responseLogin.StatusCode != System.Net.HttpStatusCode.OK || responseLogin.Content != "Logged in")
                    return false;

                SessionKey = response.Content;

                return true;
            }

            return false;            
        }

        private void PrepareRequest(RestRequest req)
        {
            var cookies = Client.CookieContainer.GetCookies(new Uri(Game.Url));
            req.AddHeader("X-CSRFToken", cookies["csrftoken"].Value);
        }

        private async Task<IRestResponse> PostJson(string url, object data)
        {
            var request = new RestRequest(url, Method.POST);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
            request.AddJsonBody(data);

            PrepareRequest(request);

            var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false); ;

            return response;
        }

        #region IGameItemProvider interface implementation

        private class itemTypesResponse
        {
            public List<string> types = null;
        }

        public string[] GetItemTypes()
        {
            if (ItemTypes == null)
            {
                //try to grab from server
            }
            if (Client != null)
            {
                var request = new RestRequest("item_types/", Method.GET);
                var response = Client.Execute(request);
                
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseDeserialized = JsonConvert.DeserializeObject<itemTypesResponse>(response.Content);
                    return responseDeserialized.types.ToArray();
                }
            }
            if (SecureProvider != null)
            {
                return SecureProvider.GetItemTypes();
            }
            return null;
        }

        private static byte[] ToByteArray(string value)
        {
            char[] charArr = value.ToCharArray();
            byte[] bytes = new byte[charArr.Length];
            for (int i = 0; i < charArr.Length; i++)
            {
                byte current = Convert.ToByte(charArr[i]);
                bytes[i] = current;
            }
            return bytes;
        }

        private class responseDict
        {
            public List<Dictionary<string,string>> items = null;
        }

        public async Task<GameItem[]> GetPlayerItems(AccountInfo account)
        {
            if (Client != null)
            {
                var request = new RestRequest(string.Format("player_items/{0},", account.ID), Method.GET);
                var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return ParseItems(response.Content);
                }
            }
            if (SecureProvider != null)
            {
                return await SecureProvider.GetPlayerItems(account).ConfigureAwait(false);
            }
            return null;
        }

        public async Task<GameItem[]> GetPlayerItems(AccountInfo account, string itemType)
        {
            if (Client != null)
            {
                var request = new RestRequest(string.Format("player_items/{0},{1}", account.ID, itemType), Method.GET);
                var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return ParseItems(response.Content);
                }
            }
            if (SecureProvider != null)
            {
                return await SecureProvider.GetPlayerItems(account, itemType).ConfigureAwait(false);
            }
            return null;
        }

        public async Task<GameItem[]> GetItems(GameItemsParams[] gameItemsParams)
        {
            if (Client != null)
            {
                var response = await PostJson("items/", gameItemsParams).ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return ParseItems(response.Content);
                }
            }
            if (SecureProvider != null)
            {
                return await SecureProvider.GetItems(gameItemsParams);
            }
            return null;
        }

        private GameItem[] ParseItems(string itemsStr)
        {
            var responseItems = JsonConvert.DeserializeObject<responseDict>(itemsStr);
            GameItem[] items = new GameItem[responseItems.items.Count];
            for (int i = 0; i < responseItems.items.Count; ++i)
            {
                string symbol = responseItems.items[i]["symbol"];
                byte[] stateBytes = ToByteArray(responseItems.items[i]["state"]);
                string stateStr = BitConverter.ToString(stateBytes);
                string contract_address = responseItems.items[i]["contract_address"];

                BaseGameItemMetadata meta = null;
                if (responseItems.items[i]["metadata"] == "ERC223")
                {
                    BigInteger balance = BigInteger.Parse(responseItems.items[i]["amount"]);
                    meta = new ERC223GameItemContract.Metadata(stateStr, contract_address, balance);
                    items[i] = new GameItem(Game, symbol, meta);
                }
                else if (responseItems.items[i]["metadata"] == "ERC721")
                {
                    BigInteger asset_id = BigInteger.Parse(responseItems.items[i]["asset_id"]);
                    meta = new ERC721GameItemContract.Metadata(contract_address, asset_id);
                    items[i] = new GameItem(Game, symbol, meta);
                    items[i].State = stateBytes;
                }
            }
            return items;
        }

        public Task<bool> Transfer(string addressFrom, string addressTo, GameItem item, ulong amount)
        {
            if (Client != null)
            {
                //TODO: implement this
                //throw new NotImplementedException();
            }
            if (SecureProvider != null)
            {
                return SecureProvider.Transfer(addressFrom, addressTo, item, amount);
            }
            return new Task<bool>(()=> { return false; });
        }

        public async Task<bool> Connect()
        {
            bool connected = false;
            //1. connect to REST server
            connected = await ConnectToGameServer();
            //2. check also fallback connector
            if (SecureProvider != null)
            {
                connected |= await SecureProvider.Connect();
            }

            return connected;
        }
        #endregion
    }
}
