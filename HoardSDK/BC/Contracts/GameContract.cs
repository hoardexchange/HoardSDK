using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hoard.BC.Contracts
{
    class GameContract
    {
        public static string ABI = @"[ { 'constant': true, 'inputs': [ { 'name': 'adr', 'type': 'address' }, { 'name': 'assetId', 'type': 'uint64' } ], 'name': 'balanceOf', 'outputs': [ { 'name': 'balance', 'type': 'uint64' } ], 'payable': false, 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_gameSrvURL', 'type': 'string' } ], 'name': 'setGameSrvURL', 'outputs': [], 'payable': false, 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'nextAssetId', 'outputs': [ { 'name': '', 'type': 'uint64' } ], 'payable': false, 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': 'assetId', 'type': 'uint64' } ], 'name': 'totalBalanceOf', 'outputs': [ { 'name': '', 'type': 'uint64' } ], 'payable': false, 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'receiver', 'type': 'address' }, { 'name': 'assetId', 'type': 'uint64' }, { 'name': 'amount', 'type': 'uint64' } ], 'name': 'transfer', 'outputs': [], 'payable': false, 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'gameSrvURL', 'outputs': [ { 'name': '', 'type': 'string' } ], 'payable': false, 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'gameId', 'outputs': [ { 'name': '', 'type': 'uint64' } ], 'payable': false, 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'from', 'type': 'address' }, { 'name': 'to', 'type': 'address' }, { 'name': 'assetId', 'type': 'uint64' }, { 'name': 'amount', 'type': 'uint64' } ], 'name': 'gameOwnerTransfer', 'outputs': [], 'payable': false, 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'adr', 'type': 'address' }, { 'name': 'amount', 'type': 'uint64' } ], 'name': 'addGameAsset', 'outputs': [], 'payable': false, 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'gameOwner', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'adr', 'type': 'address' }, { 'name': 'assetId', 'type': 'uint64' }, { 'name': 'amount', 'type': 'uint64' } ], 'name': 'burn', 'outputs': [], 'payable': false, 'type': 'function' }, { 'inputs': [ { 'name': '_gameOwner', 'type': 'address' }, { 'name': '_gameId', 'type': 'uint64' } ], 'payable': false, 'type': 'constructor' } ]";

        private readonly Web3 web3;
        private Contract contract;

        public GameContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        public Function GetFunctionNextAssetId()
        {
            return contract.GetFunction("nextAssetId");
        }

        public Function GetFunctionGameSrvURL()
        {
            return contract.GetFunction("gameSrvURL");
        }

        public Function GetFunctionSetGameSrvURL()
        {
            return contract.GetFunction("setGameSrvURL");
        }

        public Task<ulong> GetNextAssetIdAsync()
        {
            var function = GetFunctionNextAssetId();
            return function.CallAsync<ulong>();
        }

        public Task<string> GetGameServerURLAsync()
        {
            var function = GetFunctionGameSrvURL();
            return function.CallAsync<string>();
        }

        public async Task<bool> SetGameServerURLAsync(BCComm comm, string url)
        {
            var function = GetFunctionSetGameSrvURL();

            return await comm.EvaluateOnBC((address) =>
            {
                return function.SendTransactionAsync(address, new HexBigInteger(4700000), new HexBigInteger(0), url);
            });
        }
    }
}
