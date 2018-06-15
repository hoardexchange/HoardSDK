
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hoard
{
    abstract public class HoardProvider : Provider
    {
        virtual public bool Init() { return false; }

        virtual public bool IsSignedIn(PlayerID id) { return true; }

        virtual public bool SignIn(PlayerID id) { return true; }

        virtual public bool SupportsExchangeService() { return false; }

        virtual public GBDesc GetGameBackendDesc() { return null; }

        virtual public GBClient GetGameBackendClient() { return null; }

        virtual public ExchangeService GetExchangeService() { return null; }

        virtual public Task<ulong> GetGameAssetBalanceOf(string address, string tokenContractAddress)
        {
            throw new NotImplementedException();
        }

        virtual public async Task<bool> RequestPayoutPlayerReward(GameAsset gameAsset, ulong amount)
        {
            throw new NotImplementedException();
        }

        virtual public async Task<bool> RequestAssetTransferToGameContract(GameAsset gameAsset, ulong amount)
        {
            throw new NotImplementedException();
        }

        virtual public async Task<bool> RequestAssetTransfer(string to, GameAsset gameAsset, ulong amount)
        {
            throw new NotImplementedException();
        }
    }

    public class DefaultHoardProvider : HoardProvider
    {
        private HoardService hoard;
        private HoardServiceOptions options;

        public GBDesc GameBackendDesc { get; private set; }
        private BC.BCComm bcComm = null;
        public GBClient client { get; private set; } = null;

        public Dictionary<string, GameAsset> GameAssetSymbolDict { get; private set; } = new Dictionary<string, GameAsset>();

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

        public async Task<bool> GetGameAssets()
        {
            var gaContracts = await GetGameAssetContracts();

            GameAssetSymbolDict.Clear();

            ulong i = 0;
            foreach (var gac in gaContracts)
                await RegisterGameAssetContract(gac, i++);

            return true;
        }

        private async Task<object> RegisterGameAssetContract(BC.Contracts.GameAssetContract gameAssetContract, ulong assetId)
        {
            var symbol = await gameAssetContract.Symbol();

            if (!GameAssetSymbolDict.ContainsKey(symbol))
            {
                var ga = new GameAsset(
                    symbol,
                    await gameAssetContract.Name(),
                    gameAssetContract,
                    await gameAssetContract.TotalSupply(),
                    assetId,
                    await gameAssetContract.AssetType());

                GameAssetSymbolDict.Add(symbol, ga);
            }
            else
            {
                throw new Exception("Truing to register same key twice");
            }

            return null;
        }

        private async Task<BC.Contracts.GameAssetContract[]> GetGameAssetContracts()
        {
            return await bcComm.GetGameAssetContacts(GameBackendDesc.GameContract);
        }

        public override string[] getPropertyNames()
        {
            return null;
        }

        public override Result getItems(out List<GameAsset> items)
        {
            items = new List<GameAsset>();

            GetGameAssets().Wait();

            foreach (var ga in GameAssetSymbolDict)
            {
                items.Add(ga.Value);
            }

            return new Result();
        }

        public override Result getProperties(GameAsset item)
        {
            return new Result();
        }

        override public bool IsSignedIn(PlayerID id)
        {
            if (client != null)
                return client.signedPlayerID.Equals(id)
                    && client.IsSessionValid();
            else
                return false;
        }

        override public bool SignIn(PlayerID id)
        {
            if (IsSignedIn(id))
                return true;
            //create hoard client
            client = new GBClient(GameBackendDesc);

            GameExchangeService = new GameExchangeService(client, bcComm, hoard);
            GameExchangeService.Init(bcComm.GetContract<BC.Contracts.GameContract>(GameBackendDesc.GameContract));

            //connect to backend
            return client.Connect(hoard.GetAccount(id));
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
            return client;
        }

        override public ExchangeService GetExchangeService()
        {
            return GameExchangeService;
        }

        override public Task<ulong> GetGameAssetBalanceOf(string address, string tokenContractAddress)
        {
            return bcComm.GetGameAssetBalanceOf(address, tokenContractAddress);
        }

        override public async Task<bool> RequestPayoutPlayerReward(GameAsset gameAsset, ulong amount)
        {
            return await bcComm.RequestPayoutPlayerReward(
                gameAsset.ContractAddress,
                amount,
                GameBackendDesc.GameContract,
                hoard.Accounts[0].Address);
        }

        override public async Task<bool> RequestAssetTransferToGameContract(GameAsset gameAsset, ulong amount)
        {
            return await bcComm.RequestAssetTransfer(
                GameBackendDesc.GameContract,
                gameAsset.ContractAddress,
                amount,
                hoard.Accounts[0].Address);
        }

        override public async Task<bool> RequestAssetTransfer(string to, GameAsset gameAsset, ulong amount)
        {
            return await bcComm.RequestAssetTransfer(
                to,
                gameAsset.ContractAddress,
                amount,
                hoard.Accounts[0].Address);
        }

    }
}
