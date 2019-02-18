using Newtonsoft.Json;
using RestSharp;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.ExchangeServices
{
    /// <summary>
    /// Implementation of IExchangeService that communicates with Hoard Exchange services
    /// </summary>
    public class HoardExchangeService : IExchangeService
    {
        private HoardService Hoard = null;
        private RestClient Client = null;

        /// <summary>
        /// Creates new instance of <see cref="HoardExchangeService"/>
        /// </summary>
        /// <param name="hoard">Hoard service</param>
        public HoardExchangeService(HoardService hoard)
        {
            Hoard = hoard;
        }

        /// <inheritdoc/>
        public async Task<bool> Init()
        {
            if (Uri.IsWellFormedUriString(Hoard.Options.ExchangeServiceUrl, UriKind.Absolute))
            {
                Client = new RestClient(Hoard.Options.ExchangeServiceUrl);
                Client.AutomaticDecompression = false;

                //setup a cookie container for automatic cookies handling
                Client.CookieContainer = new System.Net.CookieContainer();

                return true;
            }
            ErrorCallbackProvider.Instance.ReportError($"Exchange service Url is not valid: {Hoard.Options.ExchangeServiceUrl}!");
            return false;
        }

        /// <summary>
        /// Destroys this instance
        /// </summary>
        public void Shutdown()
        {
            Client = null;
        }

        /// <inheritdoc/>
        public Task<bool> Deposit(AccountInfo account, GameItem item, BigInteger amount)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive, AccountInfo account)
        {
            var jsonStr = await GetJson(
                            string.Format("exchange/orders/{0},{1},{2}",
                            gaGet != null ? gaGet.Metadata.Get<string>("OwnerAddress") : "",
                            gaGive != null ? gaGive.Metadata.Get<string>("OwnerAddress") : "",
                            account != null ? account.ID : ""), null);

            if (!string.IsNullOrEmpty(jsonStr))
            {
                var orders = JsonConvert.DeserializeObject<Order[]>(jsonStr);
                GameItemsParams[] gameItemsParams = new GameItemsParams[orders.Length * 2];
                for (var i = 0; i < orders.Length; ++i)
                {
                    gameItemsParams[i * 2] = new GameItemsParams();
                    gameItemsParams[i * 2].ContractAddress = orders[i].tokenGive;
                    gameItemsParams[i * 2].TokenId = orders[i].tokenIdGive.ToString();

                    gameItemsParams[i * 2 + 1] = new GameItemsParams();
                    gameItemsParams[i * 2 + 1].ContractAddress = orders[i].tokenGet;
                }

                GameItem[] itemsRetrieved = await Hoard.GetItems(gameItemsParams);
                for (var i = 0; i < orders.Length; ++i)
                {
                    orders[i].UpdateGameItemObjs(itemsRetrieved[i * 2 + 1], itemsRetrieved[i * 2]);
                }

                return orders;
            }

            return new Order[0];
        }

        /// <inheritdoc/>
        public Task<bool> Order(AccountInfo account, GameItem getItem, GameItem giveItem, ulong blockTimeDuration)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> Trade(AccountInfo account, Order order)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> Withdraw(AccountInfo account, GameItem item)
        {
            throw new NotImplementedException();
        }

        private async Task<string> GetJson(string url, object data)
        {
            var request = new RestRequest(url, Method.GET);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
            request.AddJsonBody(data);

            var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false); ;

            return response.Content;
        }
    }
}
