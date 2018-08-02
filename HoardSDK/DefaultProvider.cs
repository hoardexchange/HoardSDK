using Hoard.BC.Contracts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Hoard
{
#if notdefined
    public class DefaultHoardProvider : HoardProvider
    {
        private HoardService hoard;
        private HoardServiceOptions options;
        private BC.BCComm bcComm = null;
        private Dictionary<string, IGameItemProvider> gameItemProviders = new Dictionary<string, IGameItemProvider>();

        public GameID DefaultGameID { get; private set; }
        public GBClient Client { get; private set; } = null;

        public GameExchangeService GameExchangeService { get; private set; }

        public DefaultHoardProvider(HoardService _hoard, HoardServiceOptions _options)
        {
            hoard = _hoard;
            options = _options;
        }

        override public bool Init()
        {
#if DEBUG
            Debug.WriteLine("Initializing DefaultHoardProvider GB descriptor.");
#endif
            bcComm = new BC.BCComm(options.RpcClient, hoard.Accounts[0]);

            DefaultGameID = bcComm.GetGameID(options.GameID).Result;
#if DEBUG
            Debug.WriteLine(String.Format("GameID initialized.\n {0} \n {1} \n {2}",
                DefaultGameID.ID,
                DefaultGameID.Name,
                DefaultGameID.Url
                ));
#endif
            return true;
        }

        override public void Shutdown()
        {
            if (GameExchangeService != null)
                GameExchangeService.Shutdown();

            if (Client != null)
                Client.Shutdown();

            Client = null;
            GameExchangeService = null;
            bcComm = null;
            DefaultGameID = null;
        }

        /*private GameItemContract[] GetGameItemContracts()
        {
            var contracts = new List<GameItemContract>();
            foreach (var gip in gameItemProviders)
            {
                contracts.Add(gip.Value.Contract);
            }
            return contracts.ToArray();
        }*/

        override public bool IsSignedIn(PlayerID id)
        {
            if (Client != null)
                return Client.signedPlayerID.Equals(id)
                    && Client.IsSessionValid();
            else
                return false;
        }

        override public bool SignIn(PlayerID id)
        {
            if (IsSignedIn(id))
                return true;
            //create hoard client
            Client = new GBClient(DefaultGameID);

            GameExchangeService = new GameExchangeService(Client, bcComm, hoard);
            GameExchangeService.Init(bcComm.GetContract<BC.Contracts.GameContract>(DefaultGameID.ID));

            //connect to backend
            return Client.Connect(hoard.GetAccount(id));
        }

        override public bool SupportsExchangeService()
        {
            return true;

        }

        override public GameID GetGameID()
        {
            return DefaultGameID;
        }

        override public GBClient GetGameBackendClient()
        {
            return Client;
        }

        override public ExchangeService GetExchangeService()
        {
            return GameExchangeService;
        }

        /*override public async Task<ulong> GetGameItemBalanceOf(string address, string gameItemSymbol)
        {
            return await gameItemProviders[gameItemSymbol].GetBalanceOf(address);
        }*/

        /*public void RegisterGameItemProvider(IGameItemProvider assetProvider)
        {
            gameItemProviders.Add(assetProvider.Symbol, assetProvider);
        }*/

        public bool UnregisterGameItemProvider(string assetSymbol)
        {
            return gameItemProviders.Remove(assetSymbol);
        }

        /* IProvider */

        public override GameItem[] GetGameItems(PlayerID player)
        {
            // try to get cached data from game backend server
            var getItemsTask = Client.GetJson(String.Format("items/{0}", player.ID), null);

            getItemsTask.Wait();

            string jsonStr = getItemsTask.Result;
            if (jsonStr == null)
            {
                // if not successful fetch data directly from blockchain
                List<GameItem> gameItems = new List<GameItem>();
                foreach (var gip in gameItemProviders.Values)
                {
                    gameItems.AddRange(gip.GetGameItems(player));
                }

                return gameItems.ToArray();
            }

            return JsonConvert.DeserializeObject<GameItem[]>(jsonStr);
        }

        public override void UpdateGameItemProperties(GameItem item)
        {
            IGameItemProvider itemProvider = GetGameItemProvider(item);
            if (itemProvider != null)
            {
                itemProvider.UpdateGameItemProperties(item);
            }
        }

        public override IGameItemProvider GetGameItemProvider(GameItem item)
        {
            IGameItemProvider itemProvider = null;
            gameItemProviders.TryGetValue(item.Symbol, out itemProvider);
            return itemProvider;
        }
    }
#endif
}