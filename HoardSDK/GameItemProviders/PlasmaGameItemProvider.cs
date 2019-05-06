using Hoard.BC.Contracts;
using Hoard.BC.Plasma;
using Hoard.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.GameItemProviders
{
    public class PlasmaGameItemProvider : IGameItemProvider
    {
        /// <summary>
        /// Blockchain communication interface
        /// </summary>
        protected BC.PlasmaComm plasmaComm = null;

        /// <summary>
        /// Currency (contract address) to registered GameItem contracts mapping
        /// </summary>
        protected Dictionary<string, GameItemAdapter> GameItemAdapters = new Dictionary<string, GameItemAdapter>();

        private ContractInterfaceID supportsInterfaceWithLookup = new ContractInterfaceID("0x01ffc9a7", typeof(SupportsInterfaceWithLookupContract));
        private List<ContractInterfaceID> interfaceIDs = new List<ContractInterfaceID>();

        /// <summary>
        /// Game identifier (only items for thi game will be proccessed)
        /// </summary>
        public GameID Game { get; private set; }

        /// <summary>
        /// Creates new instance of BCGameItemProvider for a particular game using supplied blockchain communication interfase
        /// </summary>
        /// <param name="game"></param>
        /// <param name="comm"></param>
        public PlasmaGameItemProvider(GameID game, IBCComm comm)
        {
            Game = game;
            plasmaComm = (BC.PlasmaComm)comm;

            RegisterContractInterfaceID(ERC223GameItemContract.InterfaceID, typeof(ERC223GameItemContract));
            RegisterContractInterfaceID(ERC721GameItemContract.InterfaceID, typeof(ERC721GameItemContract));
        }

        /// <inheritdoc/>
        public async Task<Result> Connect()
        {
            GameItemAdapters.Clear();
            return await RegisterHoardGameContracts();
        }

        /// <inheritdoc/>
        public Task<GameItem[]> GetItems(GameItemsParams[] gameItemsParams)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<bool> Transfer(Profile profileFrom, string addressTo, GameItem item, BigInteger amount)
        {
            return await GameItemAdapters[item.Symbol].Transfer(profileFrom, addressTo, item, amount);
        }

        /// <inheritdoc/>
        public string[] GetItemTypes()
        {
            return GameItemAdapters.Keys.ToArray();
        }

        /// <inheritdoc/>
        public async Task<GameItemType> GetItemTypeInfo(string itemType)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<GameItem[]> GetPlayerItems(Profile profile)
        {
            List<GameItem> items = new List<GameItem>();
            foreach (var proxy in GameItemAdapters.Values)
            {
                items.AddRange(await proxy.GetGameItems(profile.ID));
            }
            return items.ToArray();
        }

        /// <inheritdoc/>
        public async Task<GameItem[]> GetPlayerItems(Profile profile, string itemType)
        {
            List<GameItem> items = new List<GameItem>();
            if (GameItemAdapters.ContainsKey(itemType))
            {
                items.AddRange(await GameItemAdapters[itemType].GetGameItems(profile.ID));
            }
            return items.ToArray();
        }

        /// <inheritdoc/>
        public async Task<GameItem[]> GetPlayerItems(Profile profile, string itemType, ulong firstItemIndex, ulong itemsToGather)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<ulong> GetPlayerItemsAmount(Profile profile, string itemType)
        {
            if (GameItemAdapters.ContainsKey(itemType))
            {
                return (ulong)(await GameItemAdapters[itemType].GetBalanceOf(profile.ID));
            }

            return 0;
        }

        /// <summary>
        /// Registers new contract interface with particular type
        /// </summary>
        /// <param name="interfaceID">identifier of the registered interface</param>
        /// <param name="contractType">The type of constructed contracts</param>
        public void RegisterContractInterfaceID(string interfaceID, Type contractType)
        {
            interfaceIDs.Add(new ContractInterfaceID(interfaceID, contractType));
        }

        /// <summary>
        /// Helper function to automatically register all contracts for given game
        /// </summary>
        public async Task<Result> RegisterHoardGameContracts()
        {
            string[] contracts = await plasmaComm.GetGameItemContracts(Game);
            if (contracts != null)
            {
                foreach (string contractAddress in contracts)
                {
                    var proxy = await GetGameItemAdapter(contractAddress);
                    if (proxy != null)
                    {
                        var symbol = await proxy.GetSymbol();
                        System.Diagnostics.Debug.Assert(!GameItemAdapters.ContainsKey(symbol),
                            string.Format("ERROR: contract with this address has been already regisered for Game: '{0}' with ID {1}", Game.Name, Game.ID));
                        GameItemAdapters.Add(symbol, proxy);
                    }
                    else
                    {
                        return Result.InvalidContractError;
                    }
                }
                return Result.Ok;
            }

            ErrorCallbackProvider.ReportError($"Cannot find any contracts for Game: {Game.ID}!");
            return Result.ContractNotFoundError;
        }

        private async Task<GameItemAdapter> GetGameItemAdapter(string contractAddress)
        {
            SupportsInterfaceWithLookupContract interfaceContract = (SupportsInterfaceWithLookupContract)plasmaComm.GetContract(typeof(SupportsInterfaceWithLookupContract), contractAddress);

            ContractInterfaceID currentInterfaceId = null;

            if (await interfaceContract.SupportsInterface(supportsInterfaceWithLookup.InterfaceID))
            {
                foreach (ContractInterfaceID interfaceId in interfaceIDs)
                {
                    if (await interfaceContract.SupportsInterface(interfaceId.InterfaceID))
                    {
                        currentInterfaceId = interfaceId;
                    }
                }
            }

            if (currentInterfaceId != null)
            {
                var contract = plasmaComm.GetGameItemContract(Game, contractAddress, currentInterfaceId.ContractType);
                return plasmaComm.GetGameItemAdater(Game, contract);
            }

            return null;
        }
    }
}
