using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.Web3.Accounts;
using Org.BouncyCastle.Math;

namespace Hoard
{
    /// <summary>
    /// Hoard client that communicates with game backend
    /// </summary>
    class GBClient
    {
        public struct AssetInfo
        {
            public ulong game_id { get; set; }
            public ulong asset_id { get; set; }
            public ulong amount { get; set; }
            public string user_address { get; set; }
        };
        public GBDesc Description { get; private set; }
        public bool IsBusy
        {
            get { return RequestHandle != null; }
        }

        private RestClient Client = null;
        private string SessionKey = null;

        public PlayerID signedPlayerID { get; private set; } = null;

        private IList<RestResponseCookie> currentCookies = null;
        private string csrfToken = null;

        private CancellationTokenSource RequestHandle = null;
        private object locker = new object();

        public GBClient(GBDesc desc)
        {
            Description = desc;
        }

        public bool Connect(Account account)
        {
            Client = new RestClient(Description.Url);
            // GBClient = new RestClient(@"http://172.16.81.128:8000"); // Local test purpose


            RequestHandle = new CancellationTokenSource();

            //handshake

            //1. GET challenge token
            var request = new RestRequest("login/", Method.GET);
            var response = Client.Execute(request);

            if (response.ErrorException != null)
                return false;

            UpdateCookies(response.Cookies);

            string challengeToken = response.Content;
            challengeToken = challengeToken.Substring(2);


            var nonce = Hoard.Eth.Utils.Mine(challengeToken, new BigInteger("1").ShiftLeft(496));
            var nonceHex = nonce.ToString(16);

            var sig = Hoard.Eth.Utils.Sign(response.Content.Substring(2) + nonceHex, account.PrivateKey);

            var responseLogin = PostJson("login/", new
            {
                token = response.Content,
                nonce = "0x" + nonceHex,
                address = account.Address,
                signature = sig
            }).Result;

            if (responseLogin.StatusCode != System.Net.HttpStatusCode.OK || responseLogin.Content != "Logged in")
                return false;

            SessionKey = response.Content;
            UpdateCookies(responseLogin.Cookies);

            signedPlayerID = account.Address;

            ////is this needed?
            //lock (locker)
            //{
            //    RequestHandle.Dispose();
            //    RequestHandle = null;
            //}

            return true;
        }

        private void UpdateCookies(IList<RestResponseCookie> cookies)
        {
            currentCookies = cookies;
            foreach (var cookie in currentCookies)
                if (cookie.Name == "csrftoken")
                    csrfToken = cookie.Value;
        }

        private void PrepareRequest(RestRequest req)
        {
            foreach (var cookie in currentCookies)
                req.AddCookie(cookie.Name, cookie.Value);

            req.AddHeader("X-CSRFToken", csrfToken);
        }

        public async Task<T> GetData<T>(string url, object data)
        {
            var request = new RestRequest(url, Method.GET);
            request.AddJsonBody(data);

            PrepareRequest(request);

            var response = await Client.ExecuteTaskAsync<T>(request);

            return response.Data;
        }

        public async Task<string> GetJson(string url, object data)
        {
            var request = new RestRequest(url, Method.GET);
            request.AddJsonBody(data);

            PrepareRequest(request);

            var response = await Client.ExecuteTaskAsync(request);

            return response.Content;
        }

        public async Task<IRestResponse> PostJson(string url, object data)
        {
            var request = new RestRequest(url, Method.POST);
            request.AddJsonBody(data);

            PrepareRequest(request);

            var response = await Client.ExecuteTaskAsync(request);

            return response;
        }

        public bool IsSessionValid()
        {
            foreach (var cookie in currentCookies)
                if (cookie.Expired)
                    return false;

            return true;
        }

        public void Abort()
        {
            //is this needed?
            //lock (locker)
            //{
            //    if (RequestHandle != null)
            //        RequestHandle.Cancel();
            //}
        }
    }
}
