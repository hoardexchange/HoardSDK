using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Org.BouncyCastle.Math;

#if DEBUG
using System.Diagnostics;
#endif

using Nethereum.Web3.Accounts;

namespace Hoard
{
    public class HoardService
    {
        public GBDesc GameBackendDesc { get; private set;}
        public Dictionary<string, BC.Contracts.GameAssetContract> GameAssetsContracts { get; private set; } = new Dictionary<string, BC.Contracts.GameAssetContract>();
        public GameExchangeService gameExchangeService { get; private set; }

        private BC.BCComm bcComm = null;
        private GBClient client = null;
        private Dictionary<PlayerID, Account> accounts = new Dictionary<PlayerID, Account>();
        private Dictionary<string, BC.Contracts.GameCoinContract> gameCoinsContracts = new Dictionary<string, BC.Contracts.GameCoinContract>();

        public HoardService()
        {}

        public async Task<Coin[]> RequestGameCoinList()
        {
            var gameContracts = await bcComm.GetGameCoinsContacts(GameBackendDesc.GameContract);

            List<Coin> ret = new List<Coin>();

            foreach(var gc in gameContracts)
            {
                ret.Add(new Coin(await gc.Name(), await gc.Symbol(), gc.Address, await gc.TotalSupply()));
            }

            return ret.ToArray();
        }

        public async Task<GameAsset[]> RequestGameAssetList()
        {
            var gameAssetContracts = await bcComm.GetGameAssetContacts(GameBackendDesc.GameContract);

            List<GameAsset> ret = new List<GameAsset>();

            uint i = 0;
            foreach (var gac in gameAssetContracts)
            {
                ret.Add(new GameAsset(await gac.Name(), await gac.Symbol(), gac.Address, await gac.TotalSupply(), i++));
            }

            return ret.ToArray();
        }

        public async Task<GameAssetBalance[]> RequestGameAssetBalanceOf(PlayerID playerId)
        {
            //iterate for all items and get balance
            List<GameAssetBalance> assetBalancesList = new List<GameAssetBalance>();

            var gameAssets = await RequestGameAssetList();

            foreach (var ga in gameAssets)
            {
                var balance = await bcComm.GetGameAssetBalanceOf(playerId.ID, ga.ContractAddress);
                assetBalancesList.Add(new GameAssetBalance(ga, balance));
            }

            return assetBalancesList.ToArray();
        }

        public async Task<ulong> RequestGameAssetBalanceOf(string assetContractAddress, PlayerID id)
        {
            return await bcComm.GetGameAssetBalanceOf(id.ID, assetContractAddress);
        }

        public async Task<CoinBalance[]> RequestGameCoinBalanceOf(PlayerID playerId)
        {
            //iterate for all coins and get balance
            List<CoinBalance> coinBalancesList = new List<CoinBalance>();

            var gameCoins = await RequestGameCoinList();

            foreach (var gc in gameCoins)
            {
                var balance = await bcComm.GetGameCoinBalanceOf(playerId.ID, gc.ContractAddress);
                coinBalancesList.Add(new CoinBalance(gc, balance));
            }

            return coinBalancesList.ToArray();
        }

        public async Task<ulong> RequestGameCoinBalanceOf(string assetContractAddress, PlayerID id)
        {
            return await bcComm.GetGameCoinBalanceOf(id.ID, assetContractAddress);
        }

        public async Task<ulong> RequestGameCoinsBalansOf(PlayerID playerId, string coinSymbol)
        {
            if (gameCoinsContracts.ContainsKey(coinSymbol))
                return await gameCoinsContracts[coinSymbol].BalanceOf(playerId.ID);
            else
                return 0;
        }

        public bool IsSignedIn(PlayerID id)
        {
            if (client != null)
                return client.signedPlayerID.Equals(id) 
                    && client.IsSessionValid();
            else
                return false;
        }

        public bool SignIn(PlayerID id)
        {
            if (IsSignedIn(id))
                return true;
            //create hoard client
            client = new GBClient(GameBackendDesc);
            
            // Init game exchange. TODO: redesign it and make all this initialization in seprated function.
            gameExchangeService = new GameExchangeService(client, bcComm);

            //connect to backend
            return client.Connect(accounts[id]);
        }

        // PRIVATE SECTION

        /// <summary>
        /// Connects to BC and fills missing options
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool Init(HoardServiceOptions options)
        {
            InitAccounts(options.AccountsDir, options.DefaultAccountPass);

            return InitGBDescriptor(options);
        }

        private void RegisterGameCoinContract(string symbol, BC.Contracts.GameCoinContract gameCoinContract)
        {
            if (!gameCoinsContracts.ContainsKey(symbol))
            {
                gameCoinsContracts.Add(symbol, gameCoinContract);
            }
            else
            {
                throw new Exception("Truing to register same key twice");
            }
        }

        private void RegisterGameAssetContract(string symbol, BC.Contracts.GameAssetContract gameAssetContract)
        {
            if (!GameAssetsContracts.ContainsKey(symbol))
            {
                GameAssetsContracts.Add(symbol, gameAssetContract);
            }
            else
            {
                throw new Exception("Truing to register same key twice");
            }

        }

        private async Task<BC.Contracts.GameAssetContract[]> RequestGameAssetContracts()
        {
            return await bcComm.GetGameAssetContacts(GameBackendDesc.GameContract);
        }

        private async Task<BC.Contracts.GameCoinContract[]> RequestGameCoinsContracts()
        {
            return await bcComm.GetGameCoinsContacts(GameBackendDesc.GameContract);
        }

        public List<Account> Accounts
        {
            get { return accounts.Values.ToList(); }
        }

        private bool InitGBDescriptor(HoardServiceOptions options)
        {
#if DEBUG
            Debug.WriteLine("Initializing GB descriptor.");
#endif
            bcComm = new BC.BCComm(options.RpcClient, Accounts[0]);

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
            var gcContracts = RequestGameCoinsContracts().Result;

            foreach (var gc in gcContracts)
                RegisterGameCoinContract(gc.Symbol().Result, gc);

            var gaContracts = RequestGameAssetContracts().Result;

            foreach (var ga in gaContracts)
                RegisterGameAssetContract(ga.Symbol().Result, ga);

            return true;
        }

        private void InitAccounts(string path, string password) 
        {
#if DEBUG
            Debug.WriteLine(String.Format("Initializing account from path: {0}", path), "INFO");
#endif
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            var accountsFiles = ListAccountsUTCFiles(path);

            // if no account in accounts dir create one with default password.
            if (accountsFiles.Length == 0)
            {
#if DEBUG
                Debug.WriteLine("No account found. Generating one.", "INFO");
#endif
                accountsFiles = new string[1];
                accountsFiles[0] = AccountCreator.CreateAccountUTCFile(password, path);
            }

            foreach(var fileName in accountsFiles)
            {
#if DEBUG
                Debug.WriteLine(String.Format("Loading account {0}", fileName), "INFO");
#endif
                var json = File.ReadAllText(System.IO.Path.Combine(path, fileName));

                var account = Account.LoadFromKeyStore(json, password);
                this.accounts.Add(account.Address, account);
            }

#if DEBUG
            Debug.WriteLine("Accounts initialized.", "INFO");
#endif
        }

        private string[] ListAccountsUTCFiles(string path)
        {
            return Directory.GetFiles(path, "UTC--*");
        }

        public async Task<ItemCRC[]> RequestItemsCRC(GameAsset[] items)
        {
            throw new NotImplementedException();
        }

        public async Task<ItemData> RequestItemData(GameAsset id)
        {
            throw new NotImplementedException();
        }
    }
}
