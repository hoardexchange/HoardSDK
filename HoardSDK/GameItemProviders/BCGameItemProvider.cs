using Hoard.BC;
using Hoard.BC.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.GameItemProviders
{
    /// <summary>
    /// Basic provider of Game Items that communicates directly with blockchain (Ethereum)
    /// </summary>
    public class BCGameItemProvider : IGameItemProvider
    {
        /// <summary>
        /// Blockchain communication interface
        /// </summary>
        protected BCComm BCComm = null;
        /// <summary>
        /// List of registered GameItem contracts
        /// </summary>
        protected Dictionary<string, GameItemContract> ItemContracts = new Dictionary<string, GameItemContract>();

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
        public BCGameItemProvider(GameID game, BCComm comm)
        {
            Game = game;
            BCComm = comm;

            RegisterContractInterfaceID(ERC223GameItemContract.InterfaceID, typeof(ERC223GameItemContract));
            RegisterContractInterfaceID(ERC721GameItemContract.InterfaceID, typeof(ERC721GameItemContract));
        }

        #region IGameItemProvider interface implementation

        /// <inheritdoc/>
        public string[] GetItemTypes()
        {
            return ItemContracts.Keys.ToArray();
        }

        /// <inheritdoc/>
        public async Task<GameItem[]> GetPlayerItems(AccountInfo account)
        {
            List<GameItem> items = new List<GameItem>();
            foreach (var contract in ItemContracts.Values)
            {
                items.AddRange(await contract.GetGameItems(account));
            }
            return items.ToArray();
        }

        /// <inheritdoc/>
        public async Task<GameItem[]> GetPlayerItems(AccountInfo account, string itemType, ulong firstItemIndex, ulong itemsToGather)
        {
            List<GameItem> items = new List<GameItem>();
            if (ItemContracts.ContainsKey(itemType))
            {
                items.AddRange(await ItemContracts[itemType].GetGameItems(account, firstItemIndex, itemsToGather));
            }
            return items.ToArray();
        }

        /// <inheritdoc/>
        public async Task<ulong> GetPlayerItemsAmount(AccountInfo account, string itemType)
        {
            if (ItemContracts.ContainsKey(itemType))
            {
                return (ulong)(await ItemContracts[itemType].GetBalanceOf(account.ID).ConfigureAwait(false));
            }
            return await Task.FromResult<ulong>(0);
        }

        /// <inheritdoc/>
        public async Task<GameItem[]> GetPlayerItems(AccountInfo account, string itemType)
        {
            List<GameItem> items = new List<GameItem>();
            if (ItemContracts.ContainsKey(itemType))
            {
                items.AddRange(await ItemContracts[itemType].GetGameItems(account));
            }
            return items.ToArray();
        }

        /// <inheritdoc/>
        public async Task<GameItem[]> GetItems(GameItemsParams[] gameItemsParams)
        {
            List<GameItem> items = new List<GameItem>();
            foreach (var param in gameItemsParams)
            {
                GameItemContract itemContract = await GetGameItemContractByInterface(param.ContractAddress);
                items.Add(await itemContract.GetGameItem(param));
            }
            return items.ToArray();
        }

        /// <inheritdoc/>
        public Task<bool> Transfer(AccountInfo from, string addressTo, GameItem item, BigInteger amount)
        {
            GameItemContract gameItemContract = ItemContracts[item.Symbol];
            return gameItemContract.Transfer(from, addressTo, item, amount);
        }

        /// <inheritdoc/>
        public async Task<bool> Connect()
        {
            ItemContracts.Clear();
            return await RegisterHoardGameContracts();
        }
        #endregion

        /// <summary>
        /// Helper function to automatically register all contracts for given game
        /// </summary>
        public async Task<bool> RegisterHoardGameContracts()
        {
            string[] contracts = await BCComm.GetGameItemContracts(Game);
            if (contracts != null)
            {
                foreach (string c in contracts)
                {
                    GameItemContract gameItemContract = await GetGameItemContractByInterface(c);
                    if (gameItemContract != null)
                    {
                        RegisterGameItemContract(await gameItemContract.GetSymbol(), gameItemContract);
                    }
                    else
                    {
                        // TODO: handle contracts that does not implement ERC165?
                        throw new NotImplementedException();
                    }
                }
                return true;
            }
            System.Diagnostics.Trace.TraceError($"Cannot find any contracts for Game: {Game.ID}!");
            return false;
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

        private void RegisterGameItemContract(string symbol, GameItemContract contract)
        {
            System.Diagnostics.Debug.Assert(!ItemContracts.ContainsKey(symbol),
                string.Format("ERROR: contract with this symbol has been already regisered for Game: '{0}' with ID {1}",Game.Name,Game.ID));
            ItemContracts.Add(symbol, contract);
        }

        private async Task<GameItemContract> GetGameItemContractByInterface(string contractAddress)
        {
            SupportsInterfaceWithLookupContract interfaceContract = BCComm.GetContract<SupportsInterfaceWithLookupContract>(contractAddress);

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

            if(currentInterfaceId != null)
            {
                return BCComm.GetGameItemContract(Game, contractAddress, currentInterfaceId.ContractType);
            }

            return null;
        }

    }
}
