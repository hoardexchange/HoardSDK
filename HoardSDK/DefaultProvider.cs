using Hoard.BC.Contracts;
using Hoard.GameAssets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Hoard
{
    abstract public class HoardProvider : Provider
    {
        virtual public bool Init() { return false; }

        virtual public void Shutdown() { }

        virtual public bool IsSignedIn(PlayerID id) { return true; }

        virtual public bool SignIn(PlayerID id) { return true; }

        virtual public bool SupportsExchangeService() { return false; }

        virtual public GBDesc GetGameBackendDesc() { return null; }

        virtual public GBClient GetGameBackendClient() { return null; }

        virtual public ExchangeService GetExchangeService() { return null; }

        virtual public Task<ulong> GetGameAssetBalanceOf(string address, string gameAssetSymbol)
        {
            throw new NotImplementedException();
        }

        //FIXME?
        //virtual public async Task<bool> RequestPayoutPlayerReward(GameAsset gameAsset)
        //{
        //    throw new NotImplementedException();
        //}

        virtual public async Task<bool> RequestAssetTransferToGameContract(GameAsset gameAsset)
        {
            throw new NotImplementedException();
        }

        virtual public async Task<bool> RequestAssetTransfer(string to, GameAsset gameAsset)
        {
            throw new NotImplementedException();
        }
    }

    public class DefaultHoardProvider : HoardProvider
    {
        private HoardService hoard;
        private HoardServiceOptions options;
        private BC.BCComm bcComm = null;
        private Dictionary<string, IGameAssetProvider> gameAssetProviders = new Dictionary<string, IGameAssetProvider>();

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
        
        private GameAssetContract[] GetGameAssetContracts()
        {
            var contracts = new List<GameAssetContract>();
            foreach(var gap in gameAssetProviders)
            {
                contracts.Add(gap.Value.Contract);
            }
            return contracts.ToArray();
        }

        // FIXME: not needed?
        //public override string[] GetPropertyNames()
        //{
        //    return null;
        //}

        public override async Task<List<GameAsset>> GetItems(string ownerAddress, uint page, uint pageSize)
        {
            // try to get cached data from game backend server
            var jsonStr = await Client.GetJson(String.Format("items/{0}?page={1}&pageSize={2}", ownerAddress, page, pageSize), null);
            if (jsonStr != null)
            {
                return JsonConvert.DeserializeObject<GameAsset[]>(jsonStr).ToList();
            }

            // TODO: needs optimization, e.g. handle pagination
            // if server is down fetch data directly from blockchain
            List<GameAsset> gameAssets = new List<GameAsset>();
            foreach (var gap in gameAssetProviders.Values)
            {
                gameAssets.AddRange(await gap.GetItems(bcComm, ownerAddress));
            }

            return gameAssets;
        }

        // FIXME: not needed?
        //public override Result GetProperties(GameAsset item)
        //{
        //    return new Result();
        //}

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

        override public async Task<ulong> GetGameAssetBalanceOf(string address, string gameAssetSymbol)
        {
            return await gameAssetProviders[gameAssetSymbol].GetBalanceOf(address);
        }

        // FIXME: do we need this?
        //override public async Task<bool> RequestPayoutPlayerReward(GameAsset gameAsset, ulong amount)
        //{
        //    return await bcComm.RequestPayoutPlayerReward(
        //        gameAsset.ContractAddress,
        //        amount,
        //        GameBackendDesc.GameContract,
        //        hoard.Accounts[0].Address);
        //}

        override public async Task<bool> RequestAssetTransfer(string to, GameAsset gameAsset)
        {
            return await gameAssetProviders[gameAsset.AssetSymbol].Transfer(bcComm, gameAsset, hoard.Accounts[0].Address, to);
        }

        override public async Task<bool> RequestAssetTransferToGameContract(GameAsset gameAsset)
        {
            return await RequestAssetTransfer(GameBackendDesc.GameContract, gameAsset);
        }

        public void RegisterGameAssetProvider(IGameAssetProvider assetProvider)
        {
            gameAssetProviders.Add(assetProvider.AssetSymbol, assetProvider);
        }

        public bool UnregisterGameAssetProvider(string assetSymbol)
        {
            return gameAssetProviders.Remove(assetSymbol);
        }
    }
}
