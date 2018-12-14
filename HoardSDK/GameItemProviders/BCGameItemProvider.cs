using Hoard.BC;
using Hoard.BC.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.GameItemProviders
{
    /// <summary>
    /// Basic provider of Game Items that communicates directly with blockchain (Ethereum)
    /// </summary>
    public class BCGameItemProvider : IGameItemProvider
    {
        protected BCComm BCComm = null;
        protected Dictionary<string, GameItemContract> itemContracts = new Dictionary<string, GameItemContract>();
        private ContractInterfaceID supportsInterfaceWithLookup = new ContractInterfaceID("0x01ffc9a7", typeof(SupportsInterfaceWithLookupContract));
        private List<ContractInterfaceID> interfaceIDs = new List<ContractInterfaceID>();

        public GameID Game { get; private set; }

        public BCGameItemProvider(GameID game, BCComm comm)
        {
            Game = game;
            BCComm = comm;

            RegisterContractInterfaceID(ERC223GameItemContract.InterfaceID, typeof(ERC223GameItemContract));
            RegisterContractInterfaceID(ERC721GameItemContract.InterfaceID, typeof(ERC721GameItemContract));
        }

        #region IGameItemProvider interface implementation

        public string[] GetItemTypes()
        {
            return itemContracts.Keys.ToArray();
        }

        public async Task<GameItem[]> GetPlayerItems(AccountInfo account)
        {
            List<GameItem> items = new List<GameItem>();
            foreach (var contract in itemContracts.Values)
            {
                items.AddRange(await contract.GetGameItems(account));
            }
            return items.ToArray();
        }

        public async Task<GameItem[]> GetPlayerItems(AccountInfo account, string itemType)
        {
            List<GameItem> items = new List<GameItem>();
            if (itemContracts.ContainsKey(itemType))
            {
                items.AddRange(await itemContracts[itemType].GetGameItems(account));
            }
            return items.ToArray();
        }

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

        public Task<bool> Transfer(string addressFrom, string addressTo, GameItem item, ulong amount)
        {
            GameItemContract gameItemContract = itemContracts[item.Symbol];
            return gameItemContract.Transfer(addressFrom, addressTo, item, amount);
        }

        public async Task<bool> Connect()
        {
            itemContracts.Clear();
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
            return false;
        }

        public void RegisterContractInterfaceID(string interfaceID, Type contractType)
        {
            interfaceIDs.Add(new ContractInterfaceID(interfaceID, contractType));
        }

        private void RegisterGameItemContract(string symbol, GameItemContract contract)
        {
            System.Diagnostics.Debug.Assert(!itemContracts.ContainsKey(symbol),
                string.Format("ERROR: contract with this symbol has been already regisered for Game: '{0}' with ID {1}",Game.Name,Game.ID));
            itemContracts.Add(symbol, contract);
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
