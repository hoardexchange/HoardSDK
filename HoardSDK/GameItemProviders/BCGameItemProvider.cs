using Hoard.BC;
using Hoard.BC.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.GameItemProviders
{
    public class BCGameItemProvider : IGameItemProvider
    {
        private GameID Game = null;
        private BCComm BCComm = null;
        private Dictionary<string, GameItemContract> itemContracts = new Dictionary<string, GameItemContract>();
        private ContractInterfaceID supportsInterfaceWithLookup = new ContractInterfaceID("0x01ffc9a7", typeof(SupportsInterfaceWithLookupContract));
        private List<ContractInterfaceID> interfaceIDs = new List<ContractInterfaceID>();


        public BCGameItemProvider(GameID game, BCComm comm)
        {
            Game = game;
            BCComm = comm;

            RegisterContractInterfaceID("0x5713b3c1", typeof(ERC223GameItemContract));
            RegisterContractInterfaceID("0x80ac58cd", typeof(ERC721GameItemContract));
        }

        #region IGameItemProvider interface implementation

        public GameItem[] GetPlayerItems(PlayerID playerID)
        {
            List<GameItem> items = new List<GameItem>();
            foreach (var contract in itemContracts)
            {
                items.AddRange(contract.Value.GetGameItems(playerID).Result);
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
            RegisterHoardGameContracts();
            return true;
        }

        #endregion

        /// <summary>
        /// Helper function to automatically register all contracts for given game
        /// </summary>
        /// <param name="game"></param>
        public void RegisterHoardGameContracts()
        {
            string[] contracts = BCComm.GetGameItemContracts(Game).Result;
            foreach(string c in contracts)
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
        }

        public void RegisterContractInterfaceID(string interfaceID, Type contractType)
        {
            interfaceIDs.Add(new ContractInterfaceID(interfaceID, contractType));
        }

        public void RegisterGameItemContract(GameItemContract contract)
        {    
            string symbol = contract.Symbol().Result;
            itemContracts.Add(symbol, contract);
        }

        private GameItemContract GetGameItemContractByInterface(string contractAddress)
        {
            SupportsInterfaceWithLookupContract interfaceContract = BCComm.GetContract<SupportsInterfaceWithLookupContract>(contractAddress);

            if (interfaceContract.SupportsInterface(supportsInterfaceWithLookup.InterfaceID).Result)
            {
                foreach (ContractInterfaceID interfaceId in interfaceIDs)
                {
                    if (interfaceContract.SupportsInterface(interfaceId.InterfaceID).Result)
                    {
                        return BCComm.GetGameItemContract(contractAddress, interfaceId.ContractType);
                    }
                }
            }

            return null;
        }

    }
}
