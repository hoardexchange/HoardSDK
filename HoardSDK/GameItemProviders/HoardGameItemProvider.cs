using Hoard.BC.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        /// <summary>
        /// The game which manages provided Items
        /// </summary>
        public GameID Game { get; private set; }

        /// <summary>
        /// Secure provider used for validation but also in case server is down. Should be set to sth reliable like BCGameItemProvider.
        /// </summary>
        public IGameItemProvider SecureProvider { get; set; } = null;

        /// <summary>
        /// Rest client used to communicate with the server.
        /// </summary>
        protected RestClient Client = null;

        /// <summary>
        /// Cached list of all supported item types as returned from server
        /// </summary>
        private string[] ItemTypes = null;

        /// <summary>
        /// Creates new instance of Hoard GameItem provider for a particular game.
        /// This type of provider uses a GameServer as a proxy between blockchain and game client.
        /// </summary>
        /// <param name="game">GameID with valid GameServer URL</param>
        public HoardGameItemProvider(GameID game)
        {
            Game = game;
        }

        /// <summary>
        /// disconnects from game server
        /// </summary>
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

                var request = new RestRequest("", Method.GET);
                request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
                var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false);

                if (response.ErrorException != null)
                {
                    ErrorCallbackProvider.ReportError(response.ErrorException.ToString());
                    return false;
                }

                return true;
            }
            ErrorCallbackProvider.ReportError($"Not a proper game url: {Game.Url}!");
            return false;
        }

        /// <summary>
        /// Signin given account to the server. Must be done before calling endpoints protected by default challenge based authentication.
        /// </summary>
        /// <param name="account">Account ot be singed in</param>
        /// <returns></returns>
        public async Task<bool> Signin(AccountInfo account)
        {
            if (Uri.IsWellFormedUriString(Game.Url, UriKind.Absolute))
            {                
                Client = new RestClient(Game.Url);
                Client.AutomaticDecompression = false;
                //setup a cookie container for automatic cookies handling
                Client.CookieContainer = new System.Net.CookieContainer();

                //handshake

                //1. GET challenge token
                var request = new RestRequest("authentication/login/", Method.GET);
                request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
                var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false);

                if (response.ErrorException != null)
                {
                    ErrorCallbackProvider.ReportError(response.ErrorException.ToString());
                    return false;
                }
                
                string challengeToken = response.Content;
                challengeToken = challengeToken.Substring(2);

                var nonce = Eth.Utils.Mine(challengeToken, new BigInteger(1) << 496);
                var nonceHex = nonce.ToString("x");

                //generate new secure random key
                var ecKey = Nethereum.Signer.EthECKey.GenerateKey();

                var dataBytes = Encoding.ASCII.GetBytes(response.Content.Substring(2) + nonceHex);
                string sig = await account.SignMessage(dataBytes).ConfigureAwait(false);
                if (sig == null)
                    return false;

                var data = new JObject();
                data.Add("token", response.Content);
                data.Add("nonce", nonceHex.EnsureHexPrefix());
                data.Add("address", account.ID.ToString());
                data.Add("signature", sig);

                var responseLogin = await PostJson("authentication/login/", data);

                if (responseLogin.StatusCode != System.Net.HttpStatusCode.OK || responseLogin.Content != "Logged in")
                {
                    ErrorCallbackProvider.ReportError($"Failed to log in with response: {responseLogin.Content}!");
                    return false;
                }

                return true;
            }
            ErrorCallbackProvider.ReportError($"Not a proper game url: {Game.Url}!");            
            return false;            
        }

        private void PrepareRequest(RestRequest req)
        {
            var cookies = Client.CookieContainer.GetCookies(new Uri(Game.Url));
            req.AddHeader("X-CSRFToken", cookies["csrftoken"].Value);
        }

        /// <summary>
        /// Make a request to the server using POST method.
        /// </summary>
        /// <param name="url">Request Url</param>
        /// <param name="data">Optional POST params</param>
        /// <returns></returns>
        protected async Task<IRestResponse> PostJson(string url, JObject data)
        {
            var request = new RestRequest(url, Method.POST);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
            request.RequestFormat = DataFormat.Json;

            if (data != null)
                foreach (var item in data)
                {
                    request.AddParameter(item.Key, item.Value, ParameterType.GetOrPost);
                }

            PrepareRequest(request);

            var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false); ;

            return response;
        }

        #region IGameItemProvider interface implementation

        private class itemTypesResponse
        {
            public List<string> types = null;
        }

        /// <inheritdoc/>
        public string[] GetItemTypes()
        {
            if (ItemTypes != null)
            {
                return ItemTypes;
            }

            if (Client != null)
            {
                var request = new RestRequest("item_types/", Method.GET);
                var response = Client.Execute(request);
                
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseDeserialized = JsonConvert.DeserializeObject<itemTypesResponse>(response.Content);
                    ItemTypes = responseDeserialized.types.ToArray();
                }
            }
            if ((ItemTypes == null) && (SecureProvider != null))
            {
                ItemTypes = SecureProvider.GetItemTypes();
            }
            return ItemTypes;
        }

        /// <inheritdoc/>
        public async Task<GameItemType> GetItemTypeInfo(string itemType)
        {
            if (SecureProvider != null)
            {
                return await SecureProvider.GetItemTypeInfo(itemType);
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

        /// <inheritdoc/>
        public async Task<GameItem[]> GetPlayerItems(AccountInfo account)
        {
            if (Client != null)
            {
                return await GetItemsClientRequest(account.ID);
            }

            if (SecureProvider != null)
            {
                return await SecureProvider.GetPlayerItems(account).ConfigureAwait(false);
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<GameItem[]> GetPlayerItems(AccountInfo account, string itemType, ulong page, ulong itemsPerPage)
        {
            if (Client != null)
            {
                return await GetItemsClientRequest(account.ID, itemType, page, itemsPerPage);
            }

            if (SecureProvider != null)
            {
                return await SecureProvider.GetPlayerItems(account, itemType, page, itemsPerPage).ConfigureAwait(false);
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<ulong> GetPlayerItemsAmount(AccountInfo account, string itemType)
        {
            if (Client != null)
            {
                // FIXME why there is ulong instead of BigInteger in the interface?
                return await GetItemsAmountClientRequest(account.ID, itemType);
            }
            if (SecureProvider != null)
            {
                return await SecureProvider.GetPlayerItemsAmount(account, itemType).ConfigureAwait(false);
            }
            return await Task.FromResult<ulong>(0);
        }

        /// <inheritdoc/>
        public async Task<GameItem[]> GetPlayerItems(AccountInfo account, string itemType)
        {
            if (Client != null)
            {
                return await GetItemsClientRequest(account.ID, itemType);
            }

            if (SecureProvider != null)
            {
                return await SecureProvider.GetPlayerItems(account, itemType).ConfigureAwait(false);
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<GameItem[]> GetItems(GameItemsParams[] gameItemsParams)
        {
            if (Client != null)
            {
                var gameItems = new List<GameItem>();
                foreach(var gameItemsParam in gameItemsParams)
                {
                    // FIXME Do we need another endpoint for batched game item request? 
                    // We need to extend/change GameItemsParams, it is missing game item type. It has ContractAddress which is relevant for BC provider
                    gameItems.AddRange(await GetItemsClientRequest(new HoardID(gameItemsParam.PlayerAddress), null));
                }
                return gameItems.ToArray();
            }

            if (SecureProvider != null)
            {
                return await SecureProvider.GetItems(gameItemsParams);
            }

            return null;
        }

        private async Task<GameItem[]> GetItemsClientRequest(HoardID accountId = null, string itemType = null, ulong? page = null, ulong? itemsPerPage = null)
        {
            var request = new RestRequest("items/", Method.GET);

            if(accountId != null)
                request.AddQueryParameter("owner_address", accountId.ToString().EnsureHexPrefix());

            if(itemType != null)
                request.AddQueryParameter("item_type", itemType);

            if (page.HasValue)
            {
                request.AddQueryParameter("page", page.Value.ToString());
                if (itemsPerPage != null)
                    request.AddQueryParameter("per_page", itemsPerPage.Value.ToString());
            }

            var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var gameItems = new List<GameItem>();

                var result = JArray.Parse(response.Content);
                foreach (var item in result.Children<JObject>())
                {
                    gameItems.AddRange(JsonConvert.DeserializeObject<List<GameItem>>(
                        item.GetValue("items").ToString(),
                        new JsonConverter[] { new GameItemsConverter() }
                    ));
                }

                return gameItems.ToArray();
            }

            return null;
        }

        private async Task<ulong> GetItemsAmountClientRequest(HoardID accountId, string itemType = null)
        {
            var request = new RestRequest("items/balance/", Method.GET);

            request.AddQueryParameter("owner_address", accountId.ToString().EnsureHexPrefix());

            if (itemType != null)
                request.AddQueryParameter("item_type", itemType);

            var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var result = JObject.Parse(response.Content);
                return result.GetValue("balance").Value<ulong>();
            }

            return 0;
        }

        /// <inheritdoc/>
        public Task<bool> Transfer(AccountInfo from, string addressTo, GameItem item, BigInteger amount)
        {
            if (Client != null)
            {
                //TODO: implement this
                //throw new NotImplementedException();
            }

            if (SecureProvider != null)
            {
                return SecureProvider.Transfer(from, addressTo, item, amount);
            }

            ErrorCallbackProvider.ReportError("Invalid Client or SecureProvider!");
            return new Task<bool>(()=> { return false; });
        }

        /// <summary>
        /// Connects to Hoard Game Server
        /// </summary>
        /// <returns>true if connection has been established, false otherwise</returns>
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

        internal class GameItemsConverter : JsonConverter
        {
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                JToken tokens = JToken.Load(reader);
                var gameItems = new List<GameItem>();

                foreach (var token in tokens)
                {
                    if (token["contract_type"].ToString() == "ERC721")
                    {
                        string symbol = token["item_type"].ToString();
                        string contractAddress = token["contract_address"].ToString();
                        byte[] stateBytes = token["state"].ToString().HexToByteArray();
                        BigInteger tokenId = BigInteger.Parse(token["token_id"].ToString());

                        var meta = new ERC721GameItemContract.Metadata(contractAddress, tokenId);
                        var item = new GameItem(HoardService.Instance.DefaultGame, symbol, meta);
                        item.State = stateBytes;

                        gameItems.Add(item);
                    }
                    else if (token["contract_type"].ToString() == "ERC223")
                    {
                        string symbol = token["item_type"].ToString();
                        string contractAddress = token["contract_address"].ToString();
                        byte[] stateBytes = token["state"].ToString().HexToByteArray();
                        BigInteger balance = BigInteger.Parse(token["balance"].ToString());

                        var meta = new ERC223GameItemContract.Metadata(contractAddress, balance);
                        var item = new GameItem(HoardService.Instance.DefaultGame, symbol, meta);
                        item.State = stateBytes;

                        gameItems.Add(item);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }

                return gameItems;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return true;
            }

            public override bool CanRead
            {
                get { return true; }
            }

            public override bool CanWrite
            {
                get { return false; }
            }
        }
    }
}
