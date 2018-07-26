using Hoard.BC.Contracts;
using Hoard.GameItems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Hoard
{
    abstract public class HoardProvider : IProvider
    {
        virtual public bool Init() { return false; }

        virtual public void Shutdown() { }

        virtual public bool IsSignedIn(PlayerID id) { return true; }

        virtual public bool SignIn(PlayerID id) { return true; }

        virtual public bool SupportsExchangeService() { return false; }

        virtual public GBDesc GetGameBackendDesc() { return null; }

        virtual public GBClient GetGameBackendClient() { return null; }

        virtual public ExchangeService GetExchangeService() { return null; }

        virtual public Task<ulong> GetGameItemBalanceOf(string address, string gameItemSymbol)
        {
            throw new NotImplementedException();
        }

        /* IProvider */

        public abstract GameItem[] GetGameItems(PlayerID playerID);

        public abstract ItemProps GetGameItemProperties(GameItem item);

        public abstract IGameItemProvider GetGameItemProvider(GameItem item);
    }

    public class DefaultHoardProvider : HoardProvider
    {
        private HoardService hoard;
        private HoardServiceOptions options;
        private BC.BCComm bcComm = null;
        private Dictionary<string, IGameItemProvider> gameItemProviders = new Dictionary<string, IGameItemProvider>();

        public GBDesc GameBackendDesc { get; private set; }
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

            GBDesc gbDesc = bcComm.GetGBDesc(options.GameID).Result;

            GameBackendDesc = gbDesc;
#if DEBUG
            Debug.WriteLine(String.Format("GB descriptor initialized.\n {0} \n {1} \n {2} \n {3}",
                gbDesc.GameContract,
                gbDesc.GameID,
                gbDesc.Name,
                gbDesc.Url
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
            GameBackendDesc = null;
        }

        private GameItemContract[] GetGameItemContracts()
        {
            var contracts = new List<GameItemContract>();
            foreach (var gip in gameItemProviders)
            {
                contracts.Add(gip.Value.Contract);
            }
            return contracts.ToArray();
        }

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
            Client = new GBClient(GameBackendDesc);

            GameExchangeService = new GameExchangeService(Client, bcComm, hoard);
            GameExchangeService.Init(bcComm.GetContract<BC.Contracts.GameContract>(GameBackendDesc.GameContract));

            //connect to backend
            return Client.Connect(hoard.GetAccount(id));
        }

        override public bool SupportsExchangeService()
        {
            return true;

        }

        override public GBDesc GetGameBackendDesc()
        {
            return GameBackendDesc;
        }

        override public GBClient GetGameBackendClient()
        {
            return Client;
        }

        override public ExchangeService GetExchangeService()
        {
            return GameExchangeService;
        }

        override public async Task<ulong> GetGameItemBalanceOf(string address, string gameItemSymbol)
        {
            return await gameItemProviders[gameItemSymbol].GetBalanceOf(address);
        }

        public void RegisterGameItemProvider(IGameItemProvider assetProvider)
        {
            gameItemProviders.Add(assetProvider.Symbol, assetProvider);
        }

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

        public override ItemProps GetGameItemProperties(GameItem item)
        {
            IGameItemProvider itemProvider = GetGameItemProvider(item);
            if (itemProvider != null)
            {
                return itemProvider.GetGameItemProperties(item);
            }

            return null;
        }

        public override IGameItemProvider GetGameItemProvider(GameItem item)
        {
            IGameItemProvider itemProvider = null;
            gameItemProviders.TryGetValue(item.Symbol, out itemProvider);
            return itemProvider;
        }
    }
}
