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
    public class GameContract
    {
        public static string ABI = @"[ { 'constant': true, 'inputs': [ { 'name': '', 'type': 'uint64' } ], 'name': 'assetTokens', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'name', 'outputs': [ { 'name': '', 'type': 'string' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' } ,{ 'constant': false, 'inputs': [ { 'name': 'constactAddress', 'type': 'address' } ], 'name': 'addGameAssetContract', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'address' } ], 'name': 'authPlayers', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': 'adr', 'type': 'address' }, { 'name': 'assetId', 'type': 'uint64' } ], 'name': 'balanceOf', 'outputs': [ { 'name': 'balance', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_gameSrvURL', 'type': 'string' } ], 'name': 'setGameSrvURL', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'nextAssetId', 'outputs': [ { 'name': '', 'type': 'uint64' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'playerAddress', 'type': 'address' } ], 'name': 'addAuthPlayer', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'constactAddress', 'type': 'address' }, { 'name': 'assetId', 'type': 'uint64' } ], 'name': 'setGameAssetContract', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'to', 'type': 'address' }, { 'name': 'assetTokenAddress', 'type': 'address' }, { 'name': 'amount', 'type': 'uint256' } ], 'name': 'transfer', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '', 'type': 'address' }, { 'name': '', 'type': 'uint256' }, { 'name': '', 'type': 'bytes' } ], 'name': 'tokenFallback', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'assetTokenAddress', 'type': 'address' }, { 'name': 'amount', 'type': 'uint256' } ], 'name': 'payoutPlayerReward', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': 'assetId', 'type': 'uint64' } ], 'name': 'totalBalanceOf', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'assetTokenAddress', 'type': 'address' }, { 'name': 'amount', 'type': 'uint256' } ], 'name': 'withdrawFromExchange', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'gameSrvURL', 'outputs': [ { 'name': '', 'type': 'string' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_gameExchange', 'type': 'address' } ], 'name': 'setGameExchange', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'gameId', 'outputs': [ { 'name': '', 'type': 'uint64' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'gameExchange', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'gameOwner', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'inputs': [ { 'name': '_gameOwner', 'type': 'address' }, { 'name': '_gameId', 'type': 'uint64' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'constructor' } ]";

        private readonly Web3 web3;
        private Contract contract;

        public string Address { get { return contract.Address; } }

        public GameContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        private Function GetFunctionNextAssetId()
        {
            return contract.GetFunction("nextAssetId");
        }

        private Function GetFunctionGameSrvURL()
        {
            return contract.GetFunction("gameSrvURL");
        }

        private Function GetFunctionSetGameSrvURL()
        {
            return contract.GetFunction("setGameSrvURL");
        }

        private Function GetFunctionAssetTokens()
        {
            return contract.GetFunction("assetTokens");
        }

        private Function GetFunctionGameExchange()
        {
            return contract.GetFunction("gameExchange");
        }

        //FIXME?
        //private Function GetFunctionPayoutPlayerReward()
        //{
        //    return contract.GetFunction("payoutPlayerReward");
        //}

        private Function GetFunctionName()
        {
            return contract.GetFunction("name");
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

        public Task<string> GetGameAssetContractAsync(ulong assetId)
        {
            var function = GetFunctionAssetTokens();
            return function.CallAsync<string>(assetId);
        }

        public Task<string> GameExchangeContractAsync()
        {
            var function = GetFunctionGameExchange();
            return function.CallAsync<string>();
        }

        //FIXME?
        //public async Task<bool> PayoutPlayerReward(string assetTokenAddress, ulong amount, string from)
        //{
        //    var function = GetFunctionPayoutPlayerReward();
        //    var gas = await function.EstimateGasAsync(from, new Nethereum.Hex.HexTypes.HexBigInteger(100000), new Nethereum.Hex.HexTypes.HexBigInteger(0), assetTokenAddress, amount);
        //    gas = new Nethereum.Hex.HexTypes.HexBigInteger(gas.Value * 2);
        //    var receipt = await function.SendTransactionAndWaitForReceiptAsync(from, gas, new Nethereum.Hex.HexTypes.HexBigInteger(0), null, assetTokenAddress, amount);
        //    return receipt.Status.Value == 1;
        //}

        public Task<string> Name()
        {
            var function = GetFunctionName();
            return function.CallAsync<string>();
        }
    }
}

