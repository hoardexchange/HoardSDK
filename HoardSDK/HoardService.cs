using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Hoard.GameItemProviders;
using System.Numerics;

#if DEBUG
using System.Diagnostics;
#endif

using Nethereum.Web3.Accounts;

namespace Hoard
{
    /// <summary>
    /// Main service for all Hoard Platform operations.
    /// </summary>
    public sealed class HoardService
    {
        public static readonly HoardService Instance = new HoardService();
        /// <summary>
        /// Game ID with backend connection informations.
        /// </summary>
        public GameID DefaultGame { get; private set; } = GameID.kInvalidID;

        /// <summary>
        /// Default player.
        /// </summary>
        public PlayerID DefaultPlayer { get; set; } = PlayerID.kInvalidID;

        /// <summary>
        /// Game exchange service.
        /// </summary>
        public IExchangeService GameExchangeService { get; private set; } = null;

        /// <summary>
        /// List of registered GameItemProviders
        /// </summary>
        private List<IItemPropertyProvider> ItemPropertyProviders = new List<IItemPropertyProvider>();

        /// <summary>
        /// Communication channel with block chain.
        /// </summary>
        public BC.BCComm BCComm { get; private set; } = null;

        /// <summary>
        /// Accounts per PlayerID
        /// </summary>
        private Dictionary<PlayerID, Account> accounts = new Dictionary<PlayerID, Account>();

        private Dictionary<GameID, List<IGameItemProvider>> Providers = new Dictionary<GameID, List<IGameItemProvider>>();

        /// <summary>
        /// List of player accounts.
        /// </summary>
        public List<PlayerID> Players
        {
            get { return accounts.Keys.ToList(); }
        }

        /// <summary>
        /// Optimization to cancel beforefieldinit mark
        /// </summary>
        static HoardService() {}

        /// <summary>
        /// Hoard service constructor. All initilization is done in Init function.
        /// </summary>
        private HoardService()
        {
        }

        /// <summary>
        /// Request game item transfer to player.
        /// </summary>
        /// <param name="recipient">Transfer address.</param>
        /// <param name="item">Game item to be transfered.</param>
        /// <param name="amount">Amount of game item to be transfered.</param>
        /// <returns>Async task that transfer game item to the other player.</returns>
        public async Task<bool> RequestGameItemTransfer(PlayerID recipient, GameItem item, ulong amount)
        {
            IGameItemProvider gameItemProvider = GetGameItemProvider(item);
            if (gameItemProvider != null)
            {
                return await gameItemProvider.Transfer(DefaultPlayer.ID, recipient.ID, item, amount);
            }

            return false;
        }

        /// <summary>
        /// Return provider that supports given GameItem
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public IGameItemProvider GetGameItemProvider(GameItem item)
        {
            foreach (var p in Providers[item.Game])
            {
                if (p.GetItemTypes().Contains(item.Symbol))
                    return p;
            }
            return null;
        }

        /// <summary>
        /// Check if player is signed in.
        /// </summary>
        /// <param name="id">Player's id to be checked.</param>
        /// <returns>True if given player is signed in.</returns>
        public bool IsSignedIn(PlayerID id)
        {
            //TODO: this seems deprecated as each provider connects on its own
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
            //TODO: this seems deprecated as each provider connects on its own
            throw new NotImplementedException();
            /*if (!DefaultProvider.SignIn(id))
                return false;

            //GameBackendD = DefaultProvider.GetGameBackendDesc();
            GameBackendClient = DefaultProvider.GetGameBackendClient();
            GameExchangeService = DefaultProvider.GetExchangeService();

            return true;*/
        }

        /// <summary>
        /// Connects to BC and fills missing options.
        /// </summary>
        /// <param name="options">Hoard service options.</param>
        /// <returns>True if initialization succeeds.</returns>
        public bool Initialize(HoardServiceOptions options)
        {
            InitAccounts(options.AccountsDir, options.DefaultAccountPass);

            BCComm = new BC.BCComm(options.RpcClient, options.GameCenterContract); //access point to block chain - a must have

            DefaultGame = options.Game;

            //our default GameItemProvider
            if (DefaultGame != GameID.kInvalidID)
            {
                RegisterHoardGame(DefaultGame);
            }

            //init exchange service
            GameExchangeService exchange = new GameExchangeService(this);
            if (exchange.Init())
            {
                GameExchangeService = exchange;
            }

            return true;
        }

        /// <summary>
        /// Shutdown hoard service.
        /// </summary>
        public bool Shutdown()
        {
            DefaultGame = GameID.kInvalidID;
            DefaultPlayer = PlayerID.kInvalidID;
            GameExchangeService = null;
            ItemPropertyProviders.Clear();
            BCComm = null;
            ClearAccounts();
            Providers.Clear();

            return true;
        }

        /// <summary>
        /// Register default HoardBackend connector with BC fallback.
        /// </summary>
        /// <param name="game"></param>
        public bool RegisterHoardGame(GameID game)
        {
            //assumig this is a hoard game we can use a hoardconnector
            HoardGameItemProvider provider = new HoardGameItemProvider(game);//this will create REST client to communicate with backend
            //but in case server is down we will pass a fallback
            provider.FallbackConnector = new BCGameItemProvider(game,BCComm);
            if(BCComm.RegisterHoardGame(game).Result && RegisterGame(game, provider))
            {
                return true;
            }
            BCComm.UnregisterHoardGame(game);
            return false;
        }

        /// <summary>
        /// Register a connector for a particular game. Can register many connectors for a single gameID
        /// </summary>
        /// <param name="game"></param>
        /// <param name="conn"></param>
        public bool RegisterGame(GameID game, IGameItemProvider provider)
        {
            if (!Providers.ContainsKey(game) || !Providers[game].Contains(provider))
            {
                //connect to server and grab all important data (like supported item types)
                if (provider.Connect())
                {
                    //add to pool
                    if (!Providers.ContainsKey(game))
                    {
                        Providers.Add(game, new List<IGameItemProvider>());
                    }

                    Providers[game].Add(provider);

                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if game exists
        /// </summary>
        /// <param name="game"></param>
        public bool GetGameExists(GameID game)
        {
            return BCComm.GetGameExistsAsync(game.ID).Result;
        }

        /// <summary>
        /// Register provider of resolving item state and filling properties.
        /// </summary>
        /// <param name="provider">Provider to be registered.</param>
        public bool RegisterItemPropertyProvider(IItemPropertyProvider provider)
        {
            if (!ItemPropertyProviders.Contains(provider))
            {
                ItemPropertyProviders.Add(provider);
                return true;
            }
            return false;
        }

        #region PRIVATE SECTION

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
                //this.accounts.Add(new PlayerID(account.Address, account.PrivateKey), account);
                PlayerID player = new PlayerID(account.Address, account.PrivateKey, password);
                if (!accounts.ContainsKey(player))
                {
                    this.accounts.Add(player, account);
                }
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
        private IItemPropertyProvider GetItemPropertyProvider(GameItem item)
        {
            foreach (IItemPropertyProvider p in ItemPropertyProviders)
            {
                //TODO: it should not be symbol! it should be StateType!
                if (p.Supports(item.Symbol))
                    return p;
            }
            return null;
        }
        #endregion

        /// <summary>
        /// Queries Hoard Platforms for all registered games.
        /// </summary>
        /// <returns></returns>
        public async Task<GameID[]> QueryHoardGames()
        {
            //for now we only support asking BC directly, but in future we might have some Hoard servers with caching
            return await BCComm.GetHoardGames();
        }

        /// <summary>
        /// Return all registered game.
        /// </summary>
        /// <returns></returns>
        public GameID[] GetRegisteredHoardGames()
        {
            return BCComm.GetRegisteredHoardGames();
        }

        /// <summary>
        /// Returns the ethers owned by the player.
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public float GetBalance(PlayerID playerID)
        {
            try
            {
                return Decimal.ToSingle(Nethereum.Util.UnitConversion.Convert.FromWei(BCComm.GetBalance(playerID.ID).Result));
            }
            catch(Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the hoard tokens amount owned by the player.
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public BigInteger GetHRDAmount(PlayerID playerID)
        {
            try
            {
                return BCComm.GetHRDAmount(playerID.ID, GameExchangeService.GetHoardTokenAddressAsync().Result).Result;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns all Game Items owned by player in default game (one passed in options to Initialize method).
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public GameItem[] GetPlayerItems(PlayerID playerID)
        {
            return GetPlayerItems(playerID, DefaultGame);
        }

        /// <summary>
        /// Returns all Game Items owned by player in particular game
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public GameItem[] GetPlayerItems(PlayerID playerID, GameID gameID)
        {
            List<GameItem> items = new List<GameItem>();
            if (Providers.ContainsKey(gameID))
            {
                var list = Providers[gameID];
                foreach (IGameItemProvider c in list)
                {
                    items.AddRange(c.GetPlayerItems(playerID));
                }
            }
            return items.ToArray();
        }

        /// <summary>
        /// Returns all Game Items matching gameItemsParams
        /// </summary>
        /// <param name="gameItemsParams"></param>
        /// <returns></returns>
        public GameItem[] GetItems(GameItemsParams[] gameItemsParams)
        {
            List<GameItem> items = new List<GameItem>();
            foreach(var p in Providers)
            {
                foreach (IGameItemProvider provider in p.Value)
                {
                    items.AddRange(provider.GetItems(gameItemsParams));
                }
            }
            return items.ToArray();
        }

        /// <summary>
        /// Fills properties for given Game Item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool FetchItemProperties(GameItem item)
        {
            //find compatible provider
            IItemPropertyProvider pp = GetItemPropertyProvider(item);
            if (pp != null)
            {
                return pp.FetchGameItemProperties(item);
            }
            return false;
        }
    }
}
