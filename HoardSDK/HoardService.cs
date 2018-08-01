using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Hoard.GameItems;
using Hoard.DistributedStorage;
using Hoard.BC.Contracts;
using Hoard.BC;
using Hoard.BackendConnectors;

#if DEBUG
using System.Diagnostics;
#endif

using Nethereum.Web3.Accounts;

namespace Hoard
{
    public class HoardGameServerConnector : IBackendConnector
    {
        public string[] GetItemTypes(GameID game)
        {
            throw new NotImplementedException();
        }

        public GameItem[] GetPlayerItems(PlayerID playerID, string type)
        {
            //1. ask game server for items directly
            //string jsonResponse = server.GetPlayerItems(playerID, type);
            //2. Convert json to GameItems
            //return convertJSONToGameItems(jsonResponse);
            throw new NotImplementedException();
        }

        public Task<bool> Transfer(PlayerID recipient, GameItem item)
        {
            throw new NotImplementedException();
        }

        private GameItem[] convertJSONToGameItems(string jsonResponse)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Main service for all Hoard Platform operations.
    /// </summary>
    public class HoardService
    {
        /// <summary>
        /// Game ID with backend connection informations.
        /// </summary>
        public GameID DefaultGameID { get; private set; } = GameID.kInvalidID;

        /// <summary>
        /// Default player.
        /// </summary>
        public PlayerID DefaultPlayer { get; private set; } = PlayerID.kInvalidID;

        /// <summary>
        /// Game backend connection.
        /// TODO: move this to HoardGameItemProvider?
        /// </summary>
        public GBClient GameBackendClient { get; private set; } = null;

        /// <summary>
        /// Game exchange service.
        /// </summary>
        public ExchangeService GameExchangeService { get; private set; } = null;

        public BCConnector BCConnector { get; private set; } = null;

        private HoardGameItemProvider DefaultProvider = null;

        /// <summary>
        /// List of registered GameItemProviders
        /// </summary>
        private List<IGameItemProvider> GameItemProviders = new List<IGameItemProvider>();

        /// <summary>
        /// TODO: hide it in BCConnector?
        /// </summary>
        private BC.BCComm BcComm = null;

        /// <summary>
        /// Dafault provider with signin, game backend and exchange support.
        /// </summary>
        //private HoardProvider DefaultProvider = null;

        /// <summary>
        /// Accounts per PlayerID
        /// </summary>
        private Dictionary<PlayerID, Account> accounts = new Dictionary<PlayerID, Account>();

        private List<IBackendConnector> Connectors = new List<IBackendConnector>();

        /// <summary>
        /// List of player accounts.
        /// </summary>
        public List<PlayerID> Players
        {
            get { return accounts.Keys.ToList(); }
        }

        /// <summary>
        /// Hoard service constructor. All initilization is done in Init function.
        /// </summary>
        public HoardService()
        {}
        
        /// <summary>
        /// Retrives game assets using registered Providers. Beware this function is blocking and may take a long time to finish.
        /// Use RefreshGameAssets for async processing.
        /// </summary>
        /// <returns>True if operations succeeded.</returns>
        public bool RefreshGameItemsSync()
        {
            //GameAssetSymbolDict.Clear();
            //GameAssetAddressDict.Clear();

            //foreach (var providers in Providers)
            //    foreach (var provider in providers.Value)
            //    {
            //        List<IGameAssetProvider> assetProviders = provider.GetGameAssetProviders();

            //        foreach (var ap in assetProviders)
            //        {
            //            GameAssetSymbolDict.Add(ap.AssetSymbol, ap);
            //            if (ga.ContractAddress != null)
            //                GameAssetAddressDict.Add(ga.ContractAddress, ga);
            //        }
            //    }

            return true;
        }

        /// <summary>
        /// Retrives game assets using registered Providers.
        /// </summary>
        /// <returns>Async task that retives game assets.</returns>
        public async Task<bool> RefreshGameItems()
        {
            return await Task.Run(() => RefreshGameItemsSync());
        }
       
        /// <summary>
        /// Request game item transfer to player.
        /// </summary>
        /// <param name="recipient">Transfer address.</param>
        /// <param name="item">Game item to be transfered.</param>
        /// <returns>Async task that transfer game item to the other player.</returns>
        public async Task<bool> RequestGameItemTransfer(PlayerID recipient, GameItem item)
        {
            IGameItemProvider gameItemProvider = GetGameItemProvider(item);
            if (gameItemProvider != null)
            {
                return await gameItemProvider.Transfer(recipient.ID, item);
            }

            return false;
        }

        /// <summary>
        /// Check if player is signed in.
        /// </summary>
        /// <param name="id">Player's id to be checked.</param>
        /// <returns>True if given player is signed in.</returns>
        public bool IsSignedIn(PlayerID id)
        {
            throw new NotImplementedException();
            //return DefaultProvider.IsSignedIn(id);
        }

        /// <summary>
        /// Sign in given player.
        /// </summary>
        /// <param name="id">Player's id to be signed in.</param>
        /// <returns>True if given player has been successfully signed in.</returns>
        public bool SignIn(PlayerID id)
        {
            throw new NotImplementedException();
            /*if (!DefaultProvider.SignIn(id))
                return false;

            //GameBackendD = DefaultProvider.GetGameBackendDesc();
            GameBackendClient = DefaultProvider.GetGameBackendClient();
            GameExchangeService = DefaultProvider.GetExchangeService();

            return true;*/
        }

        /// <summary>
        /// Register provider of items and properties.
        /// </summary>
        /// <param name="assetType">Asset type for which this provider will be registered.</param>
        /// <param name="provider">Provider to be registered.</param>
        public bool RegisterGameItemProvider(IGameItemProvider provider)
        {
            if (!GameItemProviders.Contains(provider))
            {
                GameItemProviders.Add(provider);
                return true;
            }
            return false;
        }

        public bool RegisterConnector(IBackendConnector c)
        {
            if (Connectors.Contains(c))
                return false;
            Connectors.Add(c);
            return true;
        }

        // FIXME: not needed?
        ///// <summary>
        ///// Retrives game asset properties. Beware this function is blocking and may take a long time to finish.
        ///// Use RequestProperties for async processing.
        ///// </summary>
        ///// <param name="item">Game asset for which properties should be retrived.</param>
        ///// <returns>Operation result.</returns>
        //public Result RequestPropertiesSync(GameAsset item)
        //{
        //    List<Provider> providers = null;
        //    if (Providers.TryGetValue(item.AssetType, out providers))
        //    {
        //        foreach (var provider in providers)
        //        {
        //            return provider.GetProperties(item);
        //        }
        //    }

        //    return new Result();
        //}

        ///// <summary>
        ///// Retrives game asset properties.
        ///// </summary>
        ///// <param name="item">Game asset for which properties should be retrived.</param>
        ///// <returns>Async task that retrives game asset properties.</returns>
        //public async Task<Result> RequestProperties(GameAsset item)
        //{
        //    return await Task.Run(() => RequestPropertiesSync(item));
        //}

        ///// <summary>
        ///// Retrives game asset properties of given type. Beware this function is blocking and may take a long time to finish.
        ///// Use RequestProperties for async processing.
        ///// </summary>
        ///// <param name="item">Game asset for which properties should be retrived.</param>
        ///// <param name="name">Property type to be retrived.</param>
        ///// <returns>Operation result.</returns>
        //public Result RequestPropertiesSync(GameAsset item, string name)
        //{
        //    List<Provider> providers = null;
        //    if (Providers.TryGetValue(item.AssetType, out providers))
        //    {
        //        foreach (var provider in providers)
        //        {
        //            if (provider.GetPropertyNames().Contains<string>(name))
        //                return provider.GetProperties(item);
        //        }
        //    }

        //    return new Result();
        //}

        ///// <summary>
        ///// Retrives game asset properties of given type.
        ///// </summary>
        ///// <param name="item">Game asset for which properties should be retrived.</param>
        ///// <param name="name">Property type to be retrived.</param>
        ///// <returns>Async task that retrives game asset properties of given type.</returns>
        //public async Task<Result> RequestProperties(GameAsset item, string name)
        //{
        //    return await Task.Run(() => RequestPropertiesSync(item, name));
        //}

        // PRIVATE SECTION

        /// <summary>
        /// Connects to BC and fills missing options.
        /// </summary>
        /// <param name="options">Hoard service options.</param>
        /// <returns>True if initialization succeeds.</returns>
        public bool Initialize(HoardServiceOptions options)
        {
            InitAccounts(options.AccountsDir, options.DefaultAccountPass);

            BcComm = new BC.BCComm(options.RpcClient, GetAccount(DefaultPlayer)); //access point to block chain - a must have

            BCConnector = new BCConnector(BcComm);

            //our default GameItemProvider
            DefaultProvider = new HoardGameItemProvider(new IPFSClient(), BCConnector);
            if (!RegisterGameItemProvider(DefaultProvider))//TODO: create proper IPFS client from options?
                return false;

            return true;

            //RefreshGameItems().Wait();

            //return true;
        }

        /// <summary>
        /// Shutdown hoard service.
        /// </summary>
        public bool Shutdown()
        {
            //GameAssetSymbolDict.Clear();
            //GameAssetAddressDict.Clear();

            GameItemProviders.Clear();

            ClearAccounts();

            DefaultGameID = GameID.kInvalidID;
            GameBackendClient = null;
            GameExchangeService = null;

            return true;
        }

        public void RegisterHoardGameItems(GameID game)
        {
            //TODO: should we always get it from BC? or maybe there should be a cached data taken from elsewhere?

            BCConnector.RegisterHoardGameContracts(game);
            string[] itemTypes = BCConnector.GetItemTypes(game);
            foreach (string type in itemTypes)
                DefaultProvider.RegisterGameItemType(game, type);
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

            if (accounts.Count > 0)
                DefaultPlayer = accounts.Keys.First();

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

        /// <summary>
        /// Returns provider for given game item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private IGameItemProvider GetGameItemProvider(GameItem item)
        {
            foreach (IGameItemProvider p in GameItemProviders)
            {
                if (p.Supports(item.Symbol))
                    return p;
            }
            return null;
        }

        public GameID[] GetHoardGames()
        {
            //Should we ask BC directly or ask connector? (connector might ask hoard cache servers in first place and fallback to BC in worst case)
            return BcComm.GetHoardGames().Result;
        }

        public GameItem[] GetPlayerItems(PlayerID playerID)
        {
            return GetPlayerItems(playerID, DefaultGameID);
        }

        public GameItem[] GetPlayerItems(PlayerID playerID, GameID gameID)
        {
            List<GameItem> items = new List<GameItem>();
            foreach (IGameItemProvider p in GameItemProviders)
            {
                items.AddRange(p.GetGameItems(playerID, gameID));
            }
            return items.ToArray();
        }

        public void UpdateItemProperties(GameItem item)
        {
            //find compatible provider
            IGameItemProvider p = GetGameItemProvider(item);
            p.UpdateGameItemProperties(item);
        }
    }
}
