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
    /// <summary>
    /// Hoard Service entry point.
    /// </summary>
    public class HoardService
    {
        /// <summary>
        /// GameAsset by symbol dictionary.
        /// </summary>
        public Dictionary<string, GameAsset> GameAssetSymbolDict { get; private set; } = new Dictionary<string, GameAsset>();

        /// <summary>
        /// GameAsset by contract address dictionary.
        /// </summary>
        public Dictionary<string, GameAsset> GameAssetAddressDict { get; private set; } = new Dictionary<string, GameAsset>();

        /// <summary>
        /// GameAsset by name address dictionary.
        /// </summary>
        public Dictionary<string, GameAsset> GameAssetNameDict { get; private set; } = new Dictionary<string, GameAsset>();

        /// <summary>
        /// Game backend description with backend connection informations.
        /// </summary>
        public GBDesc GameBackendDesc { get; private set; } = new GBDesc();

        /// <summary>
        /// Game backend connection.
        /// </summary>
        public GBClient GameBackendClient { get; private set; } = null;

        /// <summary>
        /// Game exchange service.
        /// </summary>
        public ExchangeService GameExchangeService { get; private set; } = null;

        /// <summary>
        /// A list of providers for given asset type. Providers are registered using RegisterProvider.
        /// </summary>
        private Dictionary<string, List<Provider>> Providers = new Dictionary<string, List<Provider>>();

        /// <summary>
        /// Dafault provider with signin, game backend and exchange support.
        /// </summary>
        private HoardProvider DefaultProvider = null;

        /// <summary>
        /// Accounts per PlayerID
        /// </summary>
        private Dictionary<PlayerID, Account> accounts = new Dictionary<PlayerID, Account>();

        /// <summary>
        /// Hoard service constructor. All initilization is done in Init function.
        /// </summary>
        public HoardService()
        {}

        /// <summary>
        /// Return game asset by unique symbol from game assets currently cached in HoardService.
        /// </summary>
        /// <param name="symbol">Unique symbol assigned to GameAsset.</param>
        /// <returns>GameAsset by symbol.</returns>
        public GameAsset GetGameAsset(string symbol)
        {
            if (GameAssetSymbolDict.ContainsKey(symbol))
                return GameAssetSymbolDict[symbol];
            else
                return null;
        }

        /// <summary>
        /// Return all game assets currently cached in HoardService. 
        /// </summary>
        /// <returns>Array of all game assets.</returns>
        public GameAsset[] GameAssets()
        {
            return GameAssetSymbolDict.Values.ToArray();
        }

        /// <summary>
        /// Request balance of all game assets for given playerId.
        /// </summary>
        /// <param name="playerId">PlayerId for which we are requesting the balance.</param>
        /// <returns>Async task that retrives balance for all game assets belonging to the player.</returns>
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

        /// <summary>
        /// Request balance of particular game asset for given playerId.
        /// </summary>
        /// <param name="asset">Game asset to be requested for the balance.</param>
        /// <param name="playerId">PlayerId for which we are requesting the balance.</param>
        /// <returns>Async task that retrives balance for given game asset belonging to the player.</returns>
        public async Task<ulong> RequestGameAssetBalanceOf(GameAsset asset, PlayerID playerId)
        {
            return await DefaultProvider.GetGameAssetBalanceOf(playerId.ID, asset.ContractAddress);
        }

        /// <summary>
        /// Retrives game assets using registered Providers. Beware this function is blocking and may take a long time to finish.
        /// Use RefreshGameAssets for async processing.
        /// </summary>
        /// <returns>True if operations succeeded.</returns>
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

        /// <summary>
        /// Retrives game assets using registered Providers.
        /// </summary>
        /// <returns>Async task that retives game assets.</returns>
        public async Task<bool> RefreshGameAssets()
        {
            return await Task.Run(() => RefreshGameAssetsSync());
        }

        /// <summary>
        /// Gives a game asset to the player.
        /// </summary>
        /// <param name="gameAsset">Game asset to be rewarded.</param>
        /// <param name="amount">Reward amount.</param>
        /// <returns>Async task that makes player reward.</returns>
        public async Task<bool> RequestPayoutPlayerReward(GameAsset gameAsset, ulong amount)
        {
            return await DefaultProvider.RequestPayoutPlayerReward(gameAsset, amount);
        }

        /// <summary>
        /// Request asset transfer to game contract.
        /// </summary>
        /// <param name="gameAsset">Game asset to be transfered.</param>
        /// <param name="amount">Amount to transfer.</param>
        /// <returns>Async task that transfer game asset to game contract.</returns>
        public async Task<bool> RequestAssetTransferToGameContract(GameAsset gameAsset, ulong amount)
        {
            return await DefaultProvider.RequestAssetTransferToGameContract(gameAsset, amount);
        }

        /// <summary>
        /// Request asset transfer.
        /// </summary>
        /// <param name="to">Transfer address.</param>
        /// <param name="gameAsset">Game asset to be transfered.</param>
        /// <param name="amount">Amount to transfer.</param>
        /// <returns>Async task that transfer given amount of game asset to the adress.</returns>
        public async Task<bool> RequestAssetTransfer(string to, GameAsset gameAsset, ulong amount)
        {
            return await DefaultProvider.RequestAssetTransfer(to, gameAsset, amount);
        }

        /// <summary>
        /// Check if player is signed in.
        /// </summary>
        /// <param name="id">Player's id to be checked.</param>
        /// <returns>True if given player is signed in.</returns>
        public bool IsSignedIn(PlayerID id)
        {
            return DefaultProvider.IsSignedIn(id);
        }

        /// <summary>
        /// Sign in given player.
        /// </summary>
        /// <param name="id">Player's id to be signed in.</param>
        /// <returns>True if given player has been successfully signed in.</returns>
        public bool SignIn(PlayerID id)
        {
            if (!DefaultProvider.SignIn(id))
                return false;

            GameBackendDesc = DefaultProvider.GetGameBackendDesc();
            GameBackendClient = DefaultProvider.GetGameBackendClient();
            GameExchangeService = DefaultProvider.GetExchangeService();

            return true;
        }

        /// <summary>
        /// Register provider of items and properties.
        /// </summary>
        /// <param name="assetType">Asset type for which this provider will be registered.</param>
        /// <param name="provider">Provider to be registered.</param>
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

        /// <summary>
        /// Retrives game asset properties. Beware this function is blocking and may take a long time to finish.
        /// Use RequestProperties for async processing.
        /// </summary>
        /// <param name="item">Game asset for which properties should be retrived.</param>
        /// <returns>Operation result.</returns>
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

        /// <summary>
        /// Retrives game asset properties.
        /// </summary>
        /// <param name="item">Game asset for which properties should be retrived.</param>
        /// <returns>Async task that retrives game asset properties.</returns>
        public async Task<Result> RequestProperties(GameAsset item)
        {
            return await Task.Run(() => RequestPropertiesSync(item));
        }

        /// <summary>
        /// Retrives game asset properties of given type. Beware this function is blocking and may take a long time to finish.
        /// Use RequestProperties for async processing.
        /// </summary>
        /// <param name="item">Game asset for which properties should be retrived.</param>
        /// <param name="name">Property type to be retrived.</param>
        /// <returns>Operation result.</returns>
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

        /// <summary>
        /// Retrives game asset properties of given type.
        /// </summary>
        /// <param name="item">Game asset for which properties should be retrived.</param>
        /// <param name="name">Property type to be retrived.</param>
        /// <returns>Async task that retrives game asset properties of given type.</returns>
        public async Task<Result> RequestProperties(GameAsset item, string name)
        {
            return await Task.Run(() => RequestPropertiesSync(item, name));
        }

        // PRIVATE SECTION

        /// <summary>
        /// Connects to BC and fills missing options.
        /// </summary>
        /// <param name="options">Hoard service options.</param>
        /// <returns>True if initialization succeeds.</returns>
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

        /// <summary>
        /// Shutdown hoard service.
        /// </summary>
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

        /// <summary>
        /// List of player accounts.
        /// </summary>
        public List<Account> Accounts
        {
            get { return accounts.Values.ToList(); }
        }

        /// <summary>
        /// Return account for given player id.
        /// </summary>
        /// <param name="id">Player id for .</param>
        /// <returns>Player account.</returns>
        public Account GetAccount(PlayerID id)
        {
           return accounts[id];
        }

        /// <summary>
        /// Init accounts. 
        /// </summary>
        /// <param name="path">Path to directory with account files.</param>
        /// <param name="password">Account's password.</param>
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

        /// <summary>
        /// Forget any cached accounts.
        /// </summary>
        private void ClearAccounts()
        {
            accounts.Clear();
        }

        /// <summary>
        /// Return account files from given directory. Account are stored in UTC compliant files.
        /// </summary>
        /// <param name="path">path to account files.</param>
        /// <returns>Account filenames.</returns>
        private string[] ListAccountsUTCFiles(string path)
        {
            return Directory.GetFiles(path, "UTC--*");
        }
    }
}
