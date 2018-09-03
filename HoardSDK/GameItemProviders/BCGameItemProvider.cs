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

        public GameItem[] GetPlayerItems(PlayerID playerID)
        {
            List<GameItem> items = new List<GameItem>();
            foreach (var contract in itemContracts.Values)
            {
                items.AddRange(contract.GetGameItems(playerID).Result);
            }
            return items.ToArray();
        }

        public GameItem[] GetPlayerItems(PlayerID playerID, string itemType)
        {
            List<GameItem> items = new List<GameItem>();
            if (itemContracts.ContainsKey(itemType))
            {
                items.AddRange(itemContracts[itemType].GetGameItems(playerID).Result);
            }
            return items.ToArray();
        }

        public string[] GetItemTypes()
        {
            return itemContracts.Keys.ToArray();
        }

        public Task<bool> Transfer(PlayerID recipient, GameItem item)
        {
            throw new NotImplementedException();
        }

        public bool Connect()
        {
            itemContracts.Clear();
            return RegisterHoardGameContracts();
        }
        #endregion

        /// <summary>
        /// Helper function to automatically register all contracts for given game
        /// </summary>
        public bool RegisterHoardGameContracts()
        {
            string[] contracts = BCComm.GetGameItemContracts(Game).Result;
            if (contracts != null)
            {
                foreach (string c in contracts)
                {
                    GameItemContract gameItemContract = GetGameItemContractByInterface(c);
                    if (gameItemContract != null)
                    {
                        RegisterGameItemContract(gameItemContract);
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

        public void RegisterGameItemContract(GameItemContract contract)
        {
            string symbol = contract.GetSymbol().Result;
            System.Diagnostics.Debug.Assert(!itemContracts.ContainsKey(symbol),
                string.Format("ERROR: contract with this symbol has been already regisered for Game: '{0}' with ID {1}",Game.Name,Game.ID));
            itemContracts.Add(symbol, contract);
        }

        private GameItemContract GetGameItemContractByInterface(string contractAddress)
        {
            SupportsInterfaceWithLookupContract interfaceContract = BCComm.GetContract<SupportsInterfaceWithLookupContract>(contractAddress);

            ContractInterfaceID currentInterfaceId = null;

            if (interfaceContract.SupportsInterface(supportsInterfaceWithLookup.InterfaceID).Result)
            {
                foreach (ContractInterfaceID interfaceId in interfaceIDs)
                {
                    if (interfaceContract.SupportsInterface(interfaceId.InterfaceID).Result)
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
