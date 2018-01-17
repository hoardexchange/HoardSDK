using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        private CancellationTokenSource RequestHandle = null;
        private object locker = new object();

        public Client(GBDesc desc)
        {
            Description = desc;
        }

        public async Task<bool> Connect(PlayerID id)
        {
            GBClient = new RestClient(Description.Url);


            RequestHandle = new CancellationTokenSource();

            //handshake

            //1. GET challenge token
            var request = new RestRequest("login", Method.GET);
            var response = await GBClient.ExecuteTaskAsync(request, RequestHandle.Token);

            if (response.ErrorException != null)
                return false;

            string challengeToken = response.Content;

            //ulong nonce = CalculateNonce(challengeToken);

            SessionKey = response.Content;



            request.AddUrlSegment("id", id.ID.ToString());

            
            
            //var response = await GBClient.ExecuteTaskAsync(request, RequestHandle.Token);

            if (response.ErrorException != null)
                return false;

            SessionKey = response.Content;

            //is this needed?
            lock (locker)
            {
                RequestHandle.Dispose();
                RequestHandle = null;
            }

            return true;
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
