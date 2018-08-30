using Hoard.BC.Contracts;
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
        /// Fallback in case server is down. Should be set to sth more reliable like BCGameItemProvider.
        /// </summary>
        public IGameItemProvider FallbackConnector { get; set; } = null;

        private RestClient Client = null;
        private string SessionKey = null;

        public HoardGameItemProvider(GameID game)
        {
            Game = game;
        }

        public void Shutdown()
        {
        }

        private bool Connect(PlayerID player)
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
                var response = Client.Execute(request);

                if (response.ErrorException != null)
                    return false;

                //UpdateCookies(response.Cookies);

                string challengeToken = response.Content;
                challengeToken = challengeToken.Substring(2);


                var nonce = Hoard.Eth.Utils.Mine(challengeToken, new BigInteger(1) << 496);
                var nonceHex = nonce.ToString("x");

                var sig = Hoard.Eth.Utils.Sign(response.Content.Substring(2) + nonceHex, player.PrivateKey);

                var responseLogin = PostJson("login/", new
                {
                    token = response.Content,
                    nonce = "0x" + nonceHex,
                    address = player.ID,
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
            if (Client != null)
            {
                //TODO: change url to "item_types" only
                var request = new RestRequest("item_types/0x123", Method.GET);
                var response = Client.Execute(request);
                
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var responseDeserialized = JsonConvert.DeserializeObject<itemTypesResponse>(response.Content);
                    return responseDeserialized.types.ToArray();
                }
            }
            if (FallbackConnector != null)
            {
                return FallbackConnector.GetItemTypes();
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

        public GameItem[] GetPlayerItems(PlayerID playerID)
        {
            if (Client != null)
            {
                var request = new RestRequest(string.Format("player_items/{0}", playerID.ID), Method.GET);
                var response = Client.Execute(request);

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {         
                    var responseItems = JsonConvert.DeserializeObject<responseDict>(response.Content);
                    GameItem[] items = new GameItem[responseItems.items.Count];
                    for (int i=0;i<responseItems.items.Count;++i)
                    {
                        string player_address = responseItems.items[i]["player_address"];
                        string symbol = responseItems.items[i]["symbol"];
                        byte[] stateBytes = ToByteArray(responseItems.items[i]["state"]);
                        string stateStr = BitConverter.ToString(stateBytes);

                        BaseGameItemMetadata meta = null;
                        if (responseItems.items[i]["metadata"] == "ERC223")
                        {
                            BigInteger balance = BigInteger.Parse(responseItems.items[i]["amount"]);
                            meta = new ERC223GameItemContract.Metadata(stateStr, player_address, balance);
                            items[i] = new GameItem(Game, symbol, meta);
                        }
                        else if (responseItems.items[i]["metadata"] == "ERC721")
                        {
                            BigInteger asset_id = BigInteger.Parse(responseItems.items[i]["asset_id"]);
                            meta = new ERC721GameItemContract.Metadata(player_address, asset_id);
                            items[i] = new GameItem(Game, symbol, meta);
                            items[i].State = stateBytes;
                        }
                    }
                    return items;
                }
            }
            if (FallbackConnector != null)
            {
                return FallbackConnector.GetPlayerItems(playerID);
            }
            return null;
        }

        public GameItem[] GetPlayerItems(PlayerID playerID, string itemType)
        {
            //TODO
            if (FallbackConnector != null)
            {
                return FallbackConnector.GetPlayerItems(playerID, itemType);
            }
            return null;
        }

        public Task<bool> Transfer(PlayerID recipient, GameItem item)
        {
            if (Client != null)
            {
                //TODO: implement this
                //throw new NotImplementedException();
            }
            if (FallbackConnector != null)
            {
                return FallbackConnector.Transfer(recipient, item);
            }
            return new Task<bool>(()=> { return false; });
        }

        public bool Connect()
        {
            bool connected = false;
            //1. connect to REST server
            connected = Connect(HoardService.Instance.DefaultPlayer);
            //2. check also fallback connector
            if (FallbackConnector != null)
            {
                connected |= FallbackConnector.Connect();
            }
            //TODO: cut it out, this should be done from hoard.Init and then perhaphs manually per StateType not Item Symbol
            //3. register known item types to be supported by IPFSPropertyProvider

            return connected;
        }
        #endregion
    }
}
