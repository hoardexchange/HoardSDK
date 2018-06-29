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
        public Dictionary<string, GameAsset> GameAssetSymbolDict { get; private set; } = new Dictionary<string, GameAsset>();
        public Dictionary<string, GameAsset> GameAssetAddressDict { get; private set; } = new Dictionary<string, GameAsset>();
        public Dictionary<string, GameAsset> GameAssetNameDict { get; private set; } = new Dictionary<string, GameAsset>();

        public GBDesc GameBackendDesc { get; private set; } = new GBDesc();
        public GBClient GameBackendClient { get; private set; } = null;
        public ExchangeService GameExchangeService { get; private set; } = null;

        // a list of providers for given asset type
        public Dictionary<string, List<Provider>> Providers { get; private set; } = new Dictionary<string, List<Provider>>();

        // dafault provider with signin, game backend and exchange support
        private HoardProvider DefaultProvider = null;

        private Dictionary<PlayerID, Account> accounts = new Dictionary<PlayerID, Account>();

        public HoardService()
        {}

        public GameAsset GetGameAsset(string symbol)
        {
            if (GameAssetSymbolDict.ContainsKey(symbol))
                return GameAssetSymbolDict[symbol];
            else
                return null;
        }

        public GameAsset[] GameAssets()
        {
            return GameAssetSymbolDict.Values.ToArray();
        }

        public async Task<GameAssetBalance[]> RequestGameAssetsBalanceOf(PlayerID playerId)
        {
            //iterate for all items and get balance
            List<GameAssetBalance> assetBalancesList = new List<GameAssetBalance>();

            foreach (var ga in GameAssets())
            {
                if (ga.Instances!=null)
                {
                    var balance = ga.Instances.Count;
                    assetBalancesList.Add(new GameAssetBalance(ga, (ulong)ga.Instances.Count));
                }
                else
                {
                    var balance = await DefaultProvider.GetGameAssetBalanceOf(playerId.ID, ga.ContractAddress);
                    assetBalancesList.Add(new GameAssetBalance(ga, balance));
                }
            }

            return assetBalancesList.ToArray();
        }

        public async Task<ulong> RequestGameAssetBalanceOf(GameAsset asset, PlayerID id)
        {
            return await DefaultProvider.GetGameAssetBalanceOf(id.ID, asset.ContractAddress);
        }

        public bool RefreshGameAssetsSync()
        {
            GameAssetSymbolDict.Clear();
            GameAssetAddressDict.Clear();
            GameAssetNameDict.Clear();

            foreach (var providers in Providers)
                foreach (var provider in providers.Value)
                {
                    List<GameAsset> gameAssets = null;
                    provider.getItems(out gameAssets);

                    if (gameAssets!=null)
                    {
                        foreach (var ga in gameAssets)
                        {
                            GameAssetSymbolDict.Add(ga.Symbol, ga);
                            if (ga.ContractAddress!=null)
                                GameAssetAddressDict.Add(ga.ContractAddress, ga);
                            GameAssetNameDict.Add(ga.Name, ga);
                        }
                    }
                }

            return true;
        }

        public async Task<bool> RefreshGameAssets()
        {
            return await Task.Run(() => RefreshGameAssetsSync());
        }

        public async Task<bool> RequestPayoutPlayerReward(GameAsset gameAsset, ulong amount)
        {
            return await DefaultProvider.RequestPayoutPlayerReward(gameAsset, amount);
        }

        public async Task<bool> RequestAssetTransferToGameContract(GameAsset gameAsset, ulong amount)
        {
            return await DefaultProvider.RequestAssetTransferToGameContract(gameAsset, amount);
        }

        public async Task<bool> RequestAssetTransfer(string to, GameAsset gameAsset, ulong amount)
        {
            return await DefaultProvider.RequestAssetTransfer(to, gameAsset, amount);
        }

        public bool IsSignedIn(PlayerID id)
        {
            return DefaultProvider.IsSignedIn(id);
        }

        public bool SignIn(PlayerID id)
        {
            if (!DefaultProvider.SignIn(id))
                return false;

            GameBackendDesc = DefaultProvider.GetGameBackendDesc();
            GameBackendClient = DefaultProvider.GetGameBackendClient();
            GameExchangeService = DefaultProvider.GetExchangeService();

            return true;
        }

        public void RegisterProvider(string assetType, Provider provider)
        {
            List<Provider> providers = null;
            if (!Providers.TryGetValue(assetType, out providers))
            {
                providers = new List<Provider>();
                Providers[assetType] = providers;
            }

            if (!providers.Contains(provider))
            {
                Providers[assetType].Add(provider);
            }

            if (provider is HoardProvider)
            {
                DefaultProvider = provider as HoardProvider;
            }
        }

        public Result RequestPropertiesSync(GameAsset item)
        {
            List<Provider> providers = null;
            if (Providers.TryGetValue(item.AssetType, out providers))
            {
                foreach (var provider in providers)
                {
                    return provider.getProperties(item);
                }
            }

            return new Result();
        }

        public async Task<Result> RequestProperties(GameAsset item)
        {
            return await Task.Run(() => RequestPropertiesSync(item));
        }

        public Result RequestPropertiesSync(GameAsset item, string name)
        {
            List<Provider> providers = null;
            if (Providers.TryGetValue(item.AssetType, out providers))
            {
                foreach (var provider in providers)
                {
                    if (provider.getPropertyNames().Contains<string>(name))
                        return provider.getProperties(item);
                }
            }

            return new Result();
        }

        public async Task<Result> RequestProperties(GameAsset item, string name)
        {
            return await Task.Run(() => RequestPropertiesSync(item, name));
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

            if (DefaultProvider == null)
                return false;

            if (!DefaultProvider.Init())
                return false;

            RefreshGameAssets().Wait();

            return true;
        }

        public void Shutdown()
        {
            GameAssetSymbolDict.Clear();
            GameAssetAddressDict.Clear();
            GameAssetNameDict.Clear();

            Providers.Clear();

            if (DefaultProvider != null)
                DefaultProvider.Shutdown();

            ClearAccounts();

            GameBackendDesc = null;
            GameBackendClient = null;
            GameExchangeService = null;
        }

        public List<Account> Accounts
        {
            get { return accounts.Values.ToList(); }
        }

        public Account GetAccount(PlayerID id)
        {
           return accounts[id];
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

        private void ClearAccounts()
        {
            accounts.Clear();
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
