using Hoard.BC;
using Hoard.BC.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.BackendConnectors
{
    public class ContractInterfaceID
    {
        /// <summary>
        /// InterfaceID stored in 4 bytes
        /// </summary>
        public byte[] InterfaceID { get; }

        /// <summary>
        /// Contract type connected with interface ID
        /// </summary>
        public Type ContractType { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interfaceID">interfaceID 4 bytes represented in hex</param>
        public ContractInterfaceID(string interfaceID, Type contractType)
        {
            InterfaceID = BitConverter.GetBytes(Convert.ToUInt32(interfaceID, 16));

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(InterfaceID);
            }

            ContractType = contractType;
        }
    }

    public class BCConnector : IBackendConnector
    {
        private BCComm bcComm = null;
        //TODO: should this be per game?
        private Dictionary<string, GameItemContract> itemContracts = new Dictionary<string, GameItemContract>();

        private ContractInterfaceID supportsInterfaceWithLookup = new ContractInterfaceID("0x01ffc9a7", typeof(SupportsInterfaceWithLookupContract));
        private List<ContractInterfaceID> interfaceIDs = new List<ContractInterfaceID>();

        public BCConnector(BCComm comm)
        {
            bcComm = comm;

            RegisterContractInterfaceID("0x5713b3c1", typeof(ERC223GameItemContract));
            RegisterContractInterfaceID("0x80ac58cd", typeof(ERC721GameItemContract));
        }

        public string[] GetItemTypes(GameID game)
        {
            string[] types = new string[itemContracts.Count];
            int i = 0;
            foreach (GameItemContract contract in itemContracts.Values)
            {
                var task = contract.Symbol();
                task.Wait();
                types[i++] = task.Result;
            }
            return types;
        }

        /// <summary>
        /// Helper function to automatically register all contracts for given game
        /// </summary>
        /// <param name="game"></param>
        public void RegisterHoardGameContracts(GameID game)
        {
            //TODO: either this is stored in GameContract
            //or GameContract keeps only an IPFS hash of the whole list
            //or we take this information from a file

            string[] contracts = bcComm.GetGameItemContracts(game).Result;
            foreach(string c in contracts)
            {
                GameItemContract gameItemContract = GetGameItemContract(c);
                if(gameItemContract != null)
                {
                    RegisterItemContract(gameItemContract);
                }
                else
                {
                    // TODO: handle contracts that does not implement ERC165?
                }
            }
        }

        public bool RegisterItemContract(GameItemContract contract)
        {
            var task = contract.Symbol();
            task.Wait();
            if (itemContracts.ContainsKey(task.Result))
                return false;
            itemContracts.Add(task.Result, contract);
            return true;
        }

        public GameItem[] GetPlayerItems(PlayerID playerID, string symbol)
        {
            //1. contract -> Get contract based on type
            GameItemContract contract = getContractByType(symbol);
            GameItem[] items = contract.GetGameItems(playerID).Result;
            return items;
        }

        private GameItemContract getContractByType(string symbol)
        {
            return itemContracts[symbol];
        }

        public Task<bool> Transfer(PlayerID recipient, GameItem item)
        {
            throw new NotImplementedException();
        }

        public void RegisterContractInterfaceID(string interfaceID, Type contractType)
        {
            interfaceIDs.Add(new ContractInterfaceID(interfaceID, contractType));
        }

        private GameItemContract GetGameItemContract(string contractAddress)
        {
            SupportsInterfaceWithLookupContract interfaceContract = bcComm.GetContract<SupportsInterfaceWithLookupContract>(contractAddress);

            if (interfaceContract.SupportsInterface(supportsInterfaceWithLookup.InterfaceID).Result)
            {
                foreach (ContractInterfaceID interfaceId in interfaceIDs)
                {
                    if (interfaceContract.SupportsInterface(interfaceId.InterfaceID).Result)
                    {
                        return bcComm.GetGameItemContract(contractAddress, interfaceId.ContractType);
                    }
                }
            }

            return null;
        }
    }
}
