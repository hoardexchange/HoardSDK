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
    class Client
    {
        public GBDesc Description { get; private set; }
        public bool IsBusy
        {
            get { return RequestHandle != null; }
        }

        private RestClient GBClient = null;
        private string SessionKey = null;

        private IList<RestResponseCookie> currentCookies = null;
        private string csrfToken = null;

        private CancellationTokenSource RequestHandle = null;
        private object locker = new object();

        public Client(GBDesc desc)
        {
            Description = desc;
        }

        public async Task<bool> Connect(PlayerID id, Account account)
        {
            GBClient = new RestClient(Description.Url);
            // GBClient = new RestClient(@"http://172.16.81.128:8000"); // Local test purpose


            RequestHandle = new CancellationTokenSource();

            //handshake

            //1. GET challenge token
            var request = new RestRequest("login/", Method.GET);
            var response = await GBClient.ExecuteTaskAsync(request, RequestHandle.Token);

            if (response.ErrorException != null)
                return false;

            UpdateCookies(response.Cookies);

            string challengeToken = response.Content;
            challengeToken = challengeToken.Substring(2);


            var nonce = Hoard.Eth.Utils.Mine(challengeToken, new BigInteger("1").ShiftLeft(496));
            var nonceHex = nonce.ToString(16);

            var sig = Hoard.Eth.Utils.Sign(response.Content.Substring(2) + nonceHex, account.PrivateKey);


            var requestPost = new RestRequest("login/", Method.POST);
            requestPost.AddJsonBody(
            new
            {
                token = response.Content,
                nonce = "0x" + nonceHex,
                address = id.ID,
                signature = sig
            });

            PrepareRequest(requestPost);
            var responseLogin = await GBClient.ExecuteTaskAsync(requestPost, RequestHandle.Token);

            if (responseLogin.ErrorException != null)
                return false;

            SessionKey = response.Content;
            UpdateCookies(responseLogin.Cookies);

            //is this needed?
            lock (locker)
            {
                RequestHandle.Dispose();
                RequestHandle = null;
            }

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

        public async Task<object> Get(string url, object data)
        {
            var request = new RestRequest(url, Method.GET);
            request.AddJsonBody(data);

            PrepareRequest(request);

            var response = await GBClient.ExecuteTaskAsync(request);

            return response.Content;
        }

        public void Abort()
        {
            //is this needed?
            lock (locker)
            {
                if (RequestHandle != null)
                    RequestHandle.Cancel();
            }
        }
    }
}
