using Hoard.BC.Contracts;
using Hoard.DistributedStorage;
using Nethereum.Web3.Accounts;
using Newtonsoft.Json;
using Org.BouncyCastle.Math;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hoard.GameItemProviders
{
    /// <summary>
    /// Default Hoard Platform GameItem Provider supports all GameItems complying to Hoard standards
    /// </summary>
    public class HoardGameItemProvider : IGameItemProvider
    {
        public GameID Game { get; private set; }
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
            Client = new RestClient(Game.Url);
            //setup a cookie container for automatic cooki handling
            Client.CookieContainer = new System.Net.CookieContainer();

            //handshake

            //1. GET challenge token
            var request = new RestRequest("login/", Method.GET);
            var response = Client.Execute(request);

            if (response.ErrorException != null)
                return false;

            //UpdateCookies(response.Cookies);

            string challengeToken = response.Content;
            challengeToken = challengeToken.Substring(2);


            var nonce = Hoard.Eth.Utils.Mine(challengeToken, new BigInteger("1").ShiftLeft(496));
            var nonceHex = nonce.ToString(16);

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

        private void PrepareRequest(RestRequest req)
        {
            var cookies = Client.CookieContainer.GetCookies(new Uri(Game.Url));
            req.AddHeader("X-CSRFToken", cookies["csrftoken"].Value);
        }

        private async Task<IRestResponse> PostJson(string url, object data)
        {
            var request = new RestRequest(url, Method.POST);
            request.AddJsonBody(data);

            PrepareRequest(request);

            var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false); ;

            return response;
        }

        #region IGameItemProvider interface implementation

        public string[] GetItemTypes()
        {
            if (Client != null)
            {
                //TODO: implement this
                //throw new NotImplementedException();
            }
            if (FallbackConnector != null)
            {
                return FallbackConnector.GetItemTypes();
            }
            return null;
        }

        public GameItem[] GetPlayerItems(PlayerID playerID)
        {
            if (Client != null)
            {
                //TODO: implement this
                //throw new NotImplementedException();
            }
            if (FallbackConnector != null)
            {
                return FallbackConnector.GetPlayerItems(playerID);
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
