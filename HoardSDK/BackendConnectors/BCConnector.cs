using Hoard.BC;
using Hoard.BC.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.BackendConnectors
{
    public class BCConnector : IBackendConnector
    {
        private BCComm bcComm = null;
        private Dictionary<string, GameItemContract> itemContracts = new Dictionary<string, GameItemContract>();

        public BCConnector(BCComm comm)
        {
            bcComm = comm;
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
        public void RegisterAllGameContracts(GameID game)
        {
            //TODO: either this is stored in GameContract
            //or GameContract keeps only an IPFS hash of the whole list
            //or we take this information from a file

            //string[] contracts = bcComm.GetGameContracts(game);
            //foreach(string c in contracts)
            //{
            //how should we know whether to use ERC223 or ERC721
            //RegisterItemContract(bcComm.GetContract<ERC223GameItemContract>(c));
            //}
            throw new NotImplementedException();
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
            Task<GameItem[]> t = contract.GetGameItems(playerID);
            t.Wait();
            return t.Result;
        }

        private GameItemContract getContractByType(string symbol)
        {
            return itemContracts[symbol];
        }

        public Task<bool> Transfer(PlayerID recipient, GameItem item)
        {
            throw new NotImplementedException();
        }
    }
}
