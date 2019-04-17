using Hoard.ExchangeServices;
using Hoard.GameItemProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Main service for all Hoard Platform operations.
    /// </summary>
    public sealed class HoardService
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static readonly HoardService Instance = new HoardService();

        /// <summary>
        /// Initialization options.
        /// </summary>
        public HoardServiceOptions Options { get; private set; } = null;

        /// <summary>
        /// Game ID with backend connection informations.
        /// </summary>
        public GameID DefaultGame { get; private set; } = GameID.kInvalidID;

        /// <summary>
        /// Game exchange service.
        /// </summary>
        public IExchangeService ExchangeService { get; private set; } = null;

        /// <summary>
        /// List of registered GameItemProviders
        /// </summary>
        private List<IItemPropertyProvider> ItemPropertyProviders = new List<IItemPropertyProvider>();
        
        /// <summary>
        /// Communication channel with block chain.
        /// </summary>
        public BC.BCComm BCComm { get; private set; } = null;

        private Dictionary<GameID, IGameItemProvider> Providers = new Dictionary<GameID, IGameItemProvider>();

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
        /// <param name="sender">Transfer address of sender.</param>
        /// <param name="recipientID">Transfer address of recipient.</param>
        /// <param name="item">Game item to be transfered.</param>
        /// <param name="amount">Amount of game item to be transfered.</param>
        /// <returns>Async task that transfer game item to the other player.</returns>
        public async Task<bool> RequestGameItemTransfer(AccountInfo sender, HoardID recipientID, GameItem item, BigInteger amount)
        {
            IGameItemProvider gameItemProvider = GetGameItemProvider(item);
            if (gameItemProvider != null && sender != null && recipientID != null)
            {
                return await gameItemProvider.Transfer(sender, recipientID, item, amount);
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
            if (Providers.ContainsKey(item.Game))
            {
                IGameItemProvider p = Providers[item.Game];
                if (p.GetItemTypes().Contains(item.Symbol))
                    return p;
            }
            return null;
        }

        /// <summary>
        /// Return GameItem types for specyfied game
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        public string[] GetGameItemTypes(GameID game)
        {
            if (Providers.ContainsKey(game))
            {
                IGameItemProvider p = Providers[game];
                return p.GetItemTypes();
            }
            return null;
        }

        /// <summary>
        /// Connects to BC and fills missing options.
        /// </summary>
        /// <param name="options">Hoard service options.</param>
        /// <returns>Result code.</returns>
        public async Task<Result> Initialize(HoardServiceOptions options)
        {
            Options = options;

            //access point to block chain - a must have
            BCComm = new BC.BCComm(Options.RpcClient, Options.GameCenterContract);
            Tuple<bool,string> result = await BCComm.Connect();
            if (!result.Item1)
            {
                ErrorCallbackProvider.ReportError("Node not available!");
                return Result.ConnectionError;
            }

            ErrorCallbackProvider.ReportInfo(result.Item2);

            //our default GameItemProvider
            if (Options.Game != GameID.kInvalidID)
            {
                Result rslt = await RegisterHoardGame(Options.Game);
                if (rslt != Result.Ok)
                    return rslt;
            }

            DefaultGame = Options.Game;

            //init exchange service
            IExchangeService exchange = new HoardExchangeService(this);
            if (await exchange.Init())
            {
                ExchangeService = exchange;
            }

            return Result.Ok;
        }

        /// <summary>
        /// Shutdown Hoard service.
        /// </summary>
        public Result Shutdown()
        {
            DefaultGame = GameID.kInvalidID;
            ExchangeService = null;
            ItemPropertyProviders.Clear();
            BCComm = null;
            Providers.Clear();

            return Result.Ok;
        }

        /// <summary>
        /// Register default HoardBackend connector with BC fallback.
        /// </summary>
        /// <param name="game"></param>
        public async Task<Result> RegisterHoardGame(GameID game)
        {
            //assumig this is a hoard game we can use a default hoard provider that connects to Hoard game server backend
            HoardGameItemProvider provider = new HoardGameItemProvider(game);
            //for security reasons (or fallback in case server is down) we will pass a BC provider
            provider.SecureProvider = new BCGameItemProvider(game, BCComm);
            return await RegisterGame(game, provider);
        }

        /// <summary>
        /// Register a provider for a particular game. Can register only one provider for a single gameID
        /// </summary>
        /// <param name="game"></param>
        /// <param name="provider"></param>
        public async Task<Result> RegisterGame(GameID game, IGameItemProvider provider)
        {
            if (!Providers.ContainsKey(game))
            {
                //register this game in BC (this is a must for every game)
                //TODO: should we asume that all games should have a SecureProvider as BCGameItemProvider?
                //TODO: should we keep SecureProvider in original provider or parallel to it?
                //TODO: when to use SecureProvider:
                // - only when secure check should happen 
                // - when original GameItemProvider fails
                // - switch original to SecureProvider upon direct request
                Result result = await BCComm.RegisterHoardGame(game);
                if (result == Result.Ok)
                {
                    if (DefaultGame == GameID.kInvalidID)
                        DefaultGame = game;

                    result = await provider.Connect();
                    if (result == Result.Ok)
                    {
                        Providers.Add(game, provider);
                    }
                }
                return result;
            }
            else
            {
                ErrorCallbackProvider.ReportError($"Game {game.ID} already registered!");
            }
            return Result.Error;
        }

        /// <summary>
        /// Register provider of resolving item state and filling properties.
        /// </summary>
        /// <param name="provider">Provider to be registered.</param>
        public Result RegisterItemPropertyProvider(IItemPropertyProvider provider)
        {
            if (!ItemPropertyProviders.Contains(provider))
            {
                ItemPropertyProviders.Add(provider);
                return Result.Ok;
            }
            return Result.Error;
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
        /// Queries Hoard Platform for all games existing on the platform (not only in HoardService).
        /// Use carefully as it might take a while and return a long list of games.
        /// TODO: split this method into 2 methods: GetCount, QueryGames(index_from, index_to)
        /// </summary>
        /// <returns>An array of game identifiers</returns>
        public async Task<GameID[]> GetAllHoardGames()
        {
            //for now we only support asking BC directly, but in future we might have some Hoard servers with caching
            return await BCComm.GetHoardGames();
        }

        /// <summary>
        /// Retrieves number of Hoard games registered on the platform
        /// </summary>
        /// <returns></returns>
        public async Task<UInt64> GetHoardGameCount()
        {
            return await BCComm.GetHoardGameCount();
        }

        /// <summary>
        /// Check if game exists on the Hoard Platform.
        /// </summary>
        /// <param name="game"></param>
        public async Task<bool> GetGameExists(GameID game)
        {
            return await BCComm.GetGameExists(game.ID);
        }

        /// <summary>
        /// Return all registered games in HoardService.
        /// </summary>
        /// <returns></returns>
        public GameID[] GetRegisteredHoardGames()
        {
            return BCComm.GetRegisteredHoardGames();
        }

        /// <summary>
        /// Returns the ethers owned by the player.
        /// </summary>
        /// <param name="account">Account to query</param>
        /// <returns></returns>
        public async Task<float> GetBalance(AccountInfo account)
        {
            try
            {
                return decimal.ToSingle(Nethereum.Util.UnitConversion.Convert.FromWei(await BCComm.GetETHBalance(account.ID)));
            }
            catch(Exception ex)
            {
                ErrorCallbackProvider.ReportError(ex.ToString());
                return 0;
            }
        }

        /// <summary>
        /// Returns the Hoard tokens contract address.
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetHRDAddress()
        {
            try
            {
                return await BCComm.GetHRDAddress();
            }
            catch (Exception ex)
            {
                ErrorCallbackProvider.ReportError(ex.ToString());
                return "0x0";
            }
        }

        /// <summary>
        /// Returns the Hoard tokens amount owned by the player.
        /// </summary>
        /// <param name="account">Account to query</param>
        /// <returns></returns>
        public async Task<BigInteger> GetHRDAmount(AccountInfo account)
        {
            try
            {
                return await BCComm.GetHRDBalance(account.ID);
            }
            catch (Exception ex)
            {
                ErrorCallbackProvider.ReportError(ex.ToString());
                return 0;
            }
        }

        /// <summary>
        /// Returns all Game Items owned by player's subaacount in default game (one passed in options to Initialize method).
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public async Task<GameItem[]> GetPlayerItems(AccountInfo account)
        {
            return await GetPlayerItems(account, DefaultGame).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns all Game Items owned by player in default game (one passed in options to Initialize method).
        /// </summary>
        /// <param name="user">user which game items to return</param>
        /// <returns></returns>
        public async Task<GameItem[]> GetPlayerItems(User user)
        {
            return await GetPlayerItems(user, DefaultGame).ConfigureAwait(false);
        }

        /// <summary>
        /// Returns all Game Items owned by player's subaccount in particular game
        /// </summary>
        /// <param name="account"></param>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public async Task<GameItem[]> GetPlayerItems(AccountInfo account, GameID gameID)
        {
            List<GameItem> items = new List<GameItem>();
            if (Providers.ContainsKey(gameID))
            {
                IGameItemProvider c = Providers[gameID];
                items.AddRange(await c.GetPlayerItems(account).ConfigureAwait(false));
            }
            else
            {
                ErrorCallbackProvider.ReportWarning($"Game [{gameID.Name}] could not be found. Have you registered it properly?");
            }
            return items.ToArray();
        }

        /// <summary>
        /// Returns all Game Items owned by player's subaccount in particular game
        /// </summary>
        /// <param name="account"></param>
        /// <param name="gameID"></param>
        /// <param name="page">Page number</param>
        /// <param name="itemsPerPage">Number of items per page</param>
        /// <param name="itemType">Item type</param>
        /// <returns></returns>
        public async Task<GameItem[]> GetPlayerItems(AccountInfo account, GameID gameID, string itemType, ulong page, ulong itemsPerPage)
        {
            List<GameItem> items = new List<GameItem>();
            if (Providers.ContainsKey(gameID))
            {
                IGameItemProvider c = Providers[gameID];
                items.AddRange(await c.GetPlayerItems(account, itemType, page, itemsPerPage).ConfigureAwait(false));
            }
            else
            {
                ErrorCallbackProvider.ReportWarning($"Game [{gameID.Name}] could not be found. Have you registered it properly?");
            }
            return items.ToArray();
        }

        /// <summary>
        /// Returns amount of all items of the specified type belonging to a particular player with given type
        /// </summary>
        /// <param name="account"></param>
        /// <param name="gameID"></param>
        /// <param name="itemType">Item type</param>
        /// <returns></returns>
        public async Task<ulong> GetPlayerItemsAmount(AccountInfo account, GameID gameID, string itemType)
        {
            if (Providers.ContainsKey(gameID))
            {
                IGameItemProvider c = Providers[gameID];
                return await c.GetPlayerItemsAmount(account, itemType).ConfigureAwait(false);
            }
            else
            {
                ErrorCallbackProvider.ReportWarning($"Game [{gameID.Name}] could not be found. Have you registered it properly?");
            }
            return await Task.FromResult<ulong>(0);
        }

        /// <summary>
        /// Returns all Game Items owned by playerin particular game
        /// </summary>
        /// <param name="user"></param>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public async Task<GameItem[]> GetPlayerItems(User user, GameID gameID)
        {
            List<GameItem> items = new List<GameItem>();
            foreach (AccountInfo account in user.Accounts)
            {
                items.AddRange(await GetPlayerItems(account, gameID).ConfigureAwait(false));
            }
            return items.ToArray();
        }

        /// <summary>
        /// Returns all Game Items matching gameItemsParams
        /// </summary>
        /// <param name="gameItemsParams"></param>
        /// <returns></returns>
        public async Task<GameItem[]> GetItems(GameItemsParams[] gameItemsParams)
        {
            List<GameItem> items = new List<GameItem>();
            foreach(var p in Providers)
            {
                IGameItemProvider provider = p.Value;
                items.AddRange(await provider.GetItems(gameItemsParams).ConfigureAwait(false));
            }
            return items.ToArray();
        }

        /// <summary>
        /// Fills properties for given Game Item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<Result> FetchItemProperties(GameItem item)
        {
            //find compatible provider
            IItemPropertyProvider pp = GetItemPropertyProvider(item);
            if (pp != null)
            {
                return await pp.FetchGameItemProperties(item);
            }
            return Result.Error;
        }
    }
}
