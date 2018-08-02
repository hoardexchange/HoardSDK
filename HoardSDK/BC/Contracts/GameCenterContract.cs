using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Org.BouncyCastle.Math;
using System.Threading;
using System.Threading.Tasks;

namespace Hoard.BC.Contracts
{
    [FunctionOutput]
    class GameInfoDTO
    {
        [Parameter("uint64", "id", 1)]
        public ulong ID { get; set; }

        [Parameter("bytes32", "name", 2)]
        public string Name { get; set; }

    }

    /// <summary>
    /// Main Hoard contract with a list of all games registered on platform. Central point from which we can get all neccessery data.
    /// </summary>
    internal class GameCenterContract
    {        
        public static string ABI = @"[ { 'constant': true, 'inputs': [ { 'name': 'gameId', 'type': 'uint64' } ], 'name': 'getGameInfo', 'outputs': [ { 'name': 'id', 'type': 'uint64' }, { 'name': 'name', 'type': 'bytes32' } ], 'payable': false, 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'gameId', 'type': 'uint64' } ], 'name': 'removeGame', 'outputs': [], 'payable': false, 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'uint64' } ], 'name': 'gameIdsMap', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'uint64' } ], 'name': 'gameInfoMap', 'outputs': [ { 'name': 'gameId', 'type': 'uint64' }, { 'name': 'name', 'type': 'bytes32' } ], 'payable': false, 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': 'gameId', 'type': 'uint64' } ], 'name': 'getGameContact', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'owner', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'nextGameId', 'outputs': [ { 'name': '', 'type': 'uint64' } ], 'payable': false, 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'name', 'type': 'bytes32' }, { 'name': 'gameOwner', 'type': 'address' } ], 'name': 'addGame', 'outputs': [], 'payable': false, 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'uint64' } ], 'name': 'gameOwnersMap', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': 'gameId', 'type': 'uint64' } ], 'name': 'gameExists', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'type': 'function' }, { 'inputs': [], 'payable': false, 'type': 'constructor' }, { 'anonymous': false, 'inputs': [ { 'indexed': false, 'name': 'gameOwner', 'type': 'address' }, { 'indexed': false, 'name': 'gameId', 'type': 'uint64' } ], 'name': 'GameAdded', 'type': 'event' }, { 'anonymous': false, 'inputs': [ { 'indexed': false, 'name': 'gameOwner', 'type': 'address' }, { 'indexed': false, 'name': 'gameId', 'type': 'uint64' } ], 'name': 'GameRemoved', 'type': 'event' } ]";

        private readonly Web3 web3;
        private Contract contract;

        public string Address { get { return contract.Address; } }

        public GameCenterContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        public Function GetFunctionName()
        {
            return contract.GetFunction("name");
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

        /// <summary>
        /// Returns game contract address by index
        /// </summary>
        /// <param name="gameID"></param>
        /// <returns></returns>
        public Task<string> GetGameContractAsync(ulong gameID)
        {
            var function = GetFunctionGetGameContract();
            return function.CallAsync<string>(gameID);
        }

        /// <summary>
        /// Returns total number of registered games on Hoard Platform
        /// </summary>
        /// <returns></returns>
        public Task<ulong> GetGameCount()
        {
            Function function = contract.GetFunction("nextGameId");
            return function.CallAsync<ulong>();
        }

        public Task<string> NameAsync(ulong gameID)
        {
            var function = GetFunctionName();
            return function.CallAsync<string>();
        }

        public Task<bool> GetGameExistsAsync(string gameID)
        {
            var function = GetFunctionGameExists();
            return function.CallAsync<bool>(gameID);
        }

        public async Task<bool> AddGameAsync(BCComm comm, ulong id, string name, string owner)
        {
            var function = GetFunctionAddGame();

            return await comm.EvaluateOnBC((address) =>
            {
                return function.SendTransactionAsync(address, new HexBigInteger(4700000), new HexBigInteger(0), id, name, owner);
            });
        }
    }
}
