using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Org.BouncyCastle.Math;
using System.Threading.Tasks;

namespace Hoard.BC.Contracts
{
    class GameInfoDTO
    {
        [Parameter("uint64", "id", 1, true)]
        public ulong ID { get; set; }

        [Parameter("bytes32", "name", 2, true)]
        public byte[] Name { get; set; }

    }

    class GameCenterContract
    {        
        public static string ABI = @"[{'constant':true,'inputs':[{'name':'gameId','type':'uint64'}],'name':'getGameInfo','outputs':[{'name':'id','type':'uint64'},{'name':'name','type':'bytes32'}],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'gameId','type':'uint64'}],'name':'removeGame','outputs':[{'name':'','type':'bool'}],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'gameId','type':'uint64'},{'name':'name','type':'bytes32'},{'name':'gameOwner','type':'address'}],'name':'addGame','outputs':[{'name':'','type':'bool'}],'payable':false,'type':'function'},{'constant':true,'inputs':[{'name':'gameId','type':'uint64'}],'name':'getGameContact','outputs':[{'name':'','type':'address'}],'payable':false,'type':'function'},{'constant':true,'inputs':[{'name':'gameId','type':'uint64'}],'name':'gameExists','outputs':[{'name':'','type':'bool'}],'payable':false,'type':'function'},{'inputs':[],'payable':false,'type':'constructor'},{'anonymous':false,'inputs':[{'indexed':false,'name':'gameOwner','type':'address'},{'indexed':false,'name':'gameId','type':'uint64'}],'name':'GameAdded','type':'event'},{'anonymous':false,'inputs':[{'indexed':false,'name':'gameOwner','type':'address'},{'indexed':false,'name':'gameId','type':'uint64'}],'name':'GameRemoved','type':'event'}]";

        private readonly Web3 web3;
        private Contract contract;

        public GameCenterContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        public Function GetFunctionGetGameInfo()
        {
            return contract.GetFunction("getGameInfo");
        }

        public Function GetFunctionGetGameContract()
        {
            return contract.GetFunction("getGameContact");
        }

        public Function GetFunctionGameExists()
        {
            return contract.GetFunction("gameExists");
        }

        public Function GetFunctionAddGame()
        {
            return contract.GetFunction("addGame");
        }

        public Task<string> GetGameContractAsync(ulong gameID)
        {
            var function = GetFunctionGetGameContract();
            return function.CallAsync<string>(gameID);
        }

        public Task<GameInfoDTO> GetGameInfoAsync(ulong gameID)
        {
            var function = GetFunctionGetGameInfo();
            return function.CallAsync<GameInfoDTO>(gameID);
        }

        public Task<bool> GetGameExistsAsync(ulong gameID)
        {
            var function = GetFunctionGameExists();
            return function.CallAsync<bool>(gameID);
        }

        public Task<bool> AddGameAsync(ulong id, string name, string owner)
        {
            var function = GetFunctionAddGame();
            byte[] nameBin = System.Text.Encoding.UTF8.GetBytes(name);
            return function.CallAsync<bool>(id, name, owner);
        }
    }
}
