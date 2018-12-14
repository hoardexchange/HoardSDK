using Newtonsoft.Json;
using RestSharp;
using System;
using System.Threading.Tasks;

namespace Hoard.ExchangeServices
{
    public class HoardExchangeService : IExchangeService
    {
        private HoardService Hoard = null;
        private RestClient Client = null;

        private User user;
        public User User
        {
            get { return user; }
            set { user = value; }
        }

        public HoardExchangeService(HoardService hoard)
        {
            this.Hoard = hoard;
            this.user = hoard.DefaultUser;
        }

        // Setup exchange backend client. 
        // Note: Lets assume it connects on its own, independently from item providers.
        public bool Init()
        {
            if (Uri.IsWellFormedUriString(Hoard.Options.ExchangeServiceUrl, UriKind.Absolute))
            {
                Client = new RestClient(Hoard.Options.ExchangeServiceUrl);
                Client.AutomaticDecompression = false;

                //setup a cookie container for automatic cookies handling
                Client.CookieContainer = new System.Net.CookieContainer();

                return true;
            }
            return false;
        }

        public void Shutdown()
        {
            Client = null;
        }

        public Task<bool> Deposit(GameItem item, ulong amount)
        {
            throw new NotImplementedException();
        }

        public async Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive, AccountInfo account)
        {
            var jsonStr = await GetJson(
                            String.Format("exchange/orders/{0},{1},{2}",
                            gaGet != null ? gaGet.Metadata.Get<string>("OwnerAddress") : "",
                            gaGive != null ? gaGive.Metadata.Get<string>("OwnerAddress") : "",
                            account != null ? account.ID : ""), null);

            if (jsonStr != null)
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

        public Task<bool> Order(GameItem getItem, GameItem giveItem, ulong blockTimeDuration)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Trade(Order order)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Withdraw(GameItem item)
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
