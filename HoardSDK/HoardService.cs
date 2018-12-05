using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Hoard.GameItemProviders;
using System.Numerics;

using System.Diagnostics;

namespace Hoard
{
    /// <summary>
    /// Main service for all Hoard Platform operations.
    /// </summary>
    public sealed class HoardService
    {
        public static readonly HoardService Instance = new HoardService();

        public HoardServiceOptions Options { get; private set; } = null;

        /// <summary>
        /// Game ID with backend connection informations.
        /// </summary>
        public GameID DefaultGame { get; private set; } = GameID.kInvalidID;

        /// <summary>
        /// Default player.
        /// </summary>
        public User DefaultUser { get; set; } = null;

        /// <summary>
        /// Game exchange service.
        /// </summary>
        public IExchangeService GameExchangeService { get; private set; } = null;

        /// <summary>
        /// List of registered GameItemProviders
        /// </summary>
        private List<IItemPropertyProvider> ItemPropertyProviders = new List<IItemPropertyProvider>();

        /// <summary>
        /// Communication channels with hoard account services.
        /// </summary>
        private List<IAccountService> AccountServices = new List<IAccountService>();

        /// <summary>
        /// Default Account service
        /// </summary>
        public IAccountService DefaultAccountService { get; set; } = null;
        
        /// <summary>
        /// Communication channel with block chain.
        /// </summary>
        public BC.BCComm BCComm { get; private set; } = null;

        private Dictionary<GameID, List<IGameItemProvider>> Providers = new Dictionary<GameID, List<IGameItemProvider>>();

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
        public async Task<bool> RequestGameItemTransfer(User recipient, GameItem item, ulong amount)
        {
            IGameItemProvider gameItemProvider = GetGameItemProvider(item);
            if (gameItemProvider != null && DefaultUser.ActiveAccount != null && recipient.ActiveAccount != null)
            {
                return await gameItemProvider.Transfer(DefaultUser.ActiveAccount.ID, recipient.ActiveAccount.ID, item, amount);
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
        /// <param name="user">Player to be checked.</param>
        /// <returns>True if given player is signed in.</returns>
        public bool IsSignedIn(User user)
        {
            //TODO: this seems deprecated as each provider connects on its own
            throw new NotImplementedException();
            //return DefaultProvider.IsSignedIn(id);
        }

        /// <summary>
        /// Sign in given player.
        /// </summary>
        /// <param name="user">Player to be signed in.</param>
        /// <returns>True if given player has been successfully signed in.</returns>
        public bool SignIn(User user)
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
            Options = options;

            //register known account services
            AccountServices.Clear();
            AccountServices.Add(new KeyStoreAccountService(Options));
            AccountServices.Add(new HoardAccountService(Options));
            {
                IAccountService service = HW.Ledger.LedgerFactory.GetLedgerWalletAsync(HW.DerivationPath.BIP44).Result;
                if (service!=null)
                    AccountServices.Add(service);
            }
            {
                IAccountService service = HW.Trezor.TrezorFactory.GetTrezorWalletAsync(HW.DerivationPath.BIP44).Result;
                if (service!=null)
                    AccountServices.Add(service);
            }

            DefaultAccountService = AccountServices[0];

            //access point to block chain - a must have
            BCComm = new BC.BCComm(Options.RpcClient, Options.GameCenterContract);
            Tuple<bool,string> result = BCComm.Connect().Result;
            if (!result.Item1)
                return false;

            DefaultGame = Options.Game;

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
            DefaultUser = null;
            GameExchangeService = null;
            ItemPropertyProviders.Clear();
            BCComm = null;
            Providers.Clear();
            AccountServices.Clear();

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
            provider.SecureProvider = new BCGameItemProvider(game,BCComm);
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

        /// <summary>
        /// This function is work in progress: we need a complete UI solution that allows user to enter login, password when prompted
        /// as well as choose which account service to use or even which account to use.
        /// This might be a separate interface or class that creates a User
        /// There might be some functionality to change active account (requires game restart/ from start screen)
        /// Assume that it should setup default account for queries
        /// </summary>
        /// <returns></returns>
        public async Task<User> LoginPlayer()
        {
            if (Options.UserInputProvider == null)
            {
                Trace.Fail("UserInputProvider is not set. Hoard Service is not initialized properly!");
                return null;
            }

            User user = new User("player");

            // ask for hoard account identity
            user.HoardId = await Options.UserInputProvider.RequestInput(null, eUserInputType.kEmail, "Hoard account username (email)");

            //add accounts for user from all known services
            foreach (IAccountService service in AccountServices)
            {
                bool found = await service.RequestAccounts(user);
            }

            if (user.Accounts.Count > 0)
                user.SetActiveAccount(user.Accounts[0]);
            else
            {
                //TODO: ask user to choose accountservice
                IAccountService accountService = DefaultAccountService;
                AccountInfo newAccount = await accountService.CreateAccount("default", user);
                if (newAccount == null)
                    return null;
                user.SetActiveAccount(newAccount);
            }

            if (DefaultUser == null)
                DefaultUser = user;

            return user;
        }

        #region PRIVATE SECTION

        private bool IsHexString(string value)
        {
            string hx = "0123456789ABCDEF";
            foreach (char c in value.ToUpper())
            {
                if (!hx.Contains(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check last logged user address. 
        /// </summary>
        /// <param name="path">Path to directory with account file.</param>
        private string CheckLastLoggedUser(string path)
        {
            var lastUserFile = System.IO.Path.Combine(path, "lastUser.txt");
            if (File.Exists(lastUserFile))
            {
                using (StreamReader sr = File.OpenText(lastUserFile))
                {
                    string s;
                    while ((s = sr.ReadLine()) != null)
                    {
                        if (IsHexString(s))
                        {
                             return s;
                        }
                    }
                }
            }
            return "";
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
        /// Check if game exists
        /// </summary>
        /// <param name="game"></param>
        public bool GetGameExists(GameID game)
        {
            return BCComm.GetGameExistsAsync(game.ID).Result;
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
        public float GetBalance(AccountInfo account)
        {
            try
            {
                return Decimal.ToSingle(Nethereum.Util.UnitConversion.Convert.FromWei(BCComm.GetBalance(account.ID).Result));
            }
            catch(Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the hoard tokens contract address.
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public string GetHRDAddress()
        {
            try
            {
                return BCComm.GetHRDAddressAsync().Result;
            }
            catch (Exception)
            {
                return "0x0";
            }
        }

        /// <summary>
        /// Returns the hoard tokens amount owned by the player.
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public BigInteger GetHRDAmount(AccountInfo info)
        {
            try
            {
                return BCComm.GetHRDAmountAsync(info.ID).Result;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns all Game Items owned by player's subaacount in default game (one passed in options to Initialize method).
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public GameItem[] GetPlayerItems(AccountInfo account)
        {
            return GetPlayerItems(account, DefaultGame);
        }

        /// <summary>
        /// Returns all Game Items owned by player in default game (one passed in options to Initialize method).
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public GameItem[] GetPlayerItems(User user)
        {
            return GetPlayerItems(user, DefaultGame);
        }

        /// <summary>
        /// Returns all Game Items owned by player's subaccount in particular game
        /// </summary>
        /// <param name="account"></param>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public GameItem[] GetPlayerItems(AccountInfo account, GameID gameID)
        {
            List<GameItem> items = new List<GameItem>();
            if (Providers.ContainsKey(gameID))
            {
                var list = Providers[gameID];
                foreach (IGameItemProvider c in list)
                {
                    items.AddRange(c.GetPlayerItems(account));
                }
            }
            return items.ToArray();
        }

        /// <summary>
        /// Returns all Game Items owned by playerin particular game
        /// </summary>
        /// <param name="user"></param>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public GameItem[] GetPlayerItems(User user, GameID gameID)
        {
            List<GameItem> items = new List<GameItem>();
            foreach (AccountInfo account in user.Accounts)
            {
                items.AddRange(GetPlayerItems(account, gameID));
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
