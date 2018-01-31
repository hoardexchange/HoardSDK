﻿using Nethereum.Contracts;
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
        public static string ABI = @"[ { 'constant': true, 'inputs': [], 'name': 'gameCoinsContactsLength', 'outputs': [ { 'name': '', 'type': 'uint64' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'uint256' } ], 'name': 'gameCoinsContacts', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': 'adr', 'type': 'address' }, { 'name': 'assetId', 'type': 'uint64' } ], 'name': 'balanceOf', 'outputs': [ { 'name': 'balance', 'type': 'uint64' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_gameSrvURL', 'type': 'string' } ], 'name': 'setGameSrvURL', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'nextAssetId', 'outputs': [ { 'name': '', 'type': 'uint64' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'adr', 'type': 'address' } ], 'name': 'addGameCoinContract', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'adr', 'type': 'address' }, { 'name': 'index', 'type': 'uint64' } ], 'name': 'setGameCoinContract', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': 'assetId', 'type': 'uint64' } ], 'name': 'totalBalanceOf', 'outputs': [ { 'name': '', 'type': 'uint64' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'receiver', 'type': 'address' }, { 'name': 'assetId', 'type': 'uint64' }, { 'name': 'amount', 'type': 'uint64' } ], 'name': 'transfer', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'gameSrvURL', 'outputs': [ { 'name': '', 'type': 'string' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'gameId', 'outputs': [ { 'name': '', 'type': 'uint64' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'from', 'type': 'address' }, { 'name': 'to', 'type': 'address' }, { 'name': 'assetId', 'type': 'uint64' }, { 'name': 'amount', 'type': 'uint64' } ], 'name': 'gameOwnerTransfer', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'adr', 'type': 'address' }, { 'name': 'amount', 'type': 'uint64' } ], 'name': 'addGameAsset', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'gameOwner', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'adr', 'type': 'address' }, { 'name': 'assetId', 'type': 'uint64' }, { 'name': 'amount', 'type': 'uint64' } ], 'name': 'burn', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'inputs': [ { 'name': '_gameOwner', 'type': 'address' }, { 'name': '_gameId', 'type': 'uint64' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'constructor' } ]";

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

        public Function GetFunctionBalanceOf()
        {
            return contract.GetFunction("balanceOf");
        }

        public Function GetFunctionGameSrvURL()
        {
            return contract.GetFunction("gameSrvURL");
        }

        public Function GetFunctionSetGameSrvURL()
        {
            return contract.GetFunction("setGameSrvURL");
        }

        public Function GetFunctionGameCoinsContacts()
        {
            return contract.GetFunction("gameCoinsContacts");
        }

        public Function GetFunctionGameCoinsContactsLength()
        {
            return contract.GetFunction("gameCoinsContactsLength");
        }

        public Task<ulong> GetNextAssetIdAsync()
        {
            var function = GetFunctionNextAssetId();
            return function.CallAsync<ulong>();
        }

        public Task<ulong> GetItemBalance(string address, ulong itemID)
        {
            var function = GetFunctionBalanceOf();
            return function.CallAsync<ulong>(address, itemID);
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

        public Task<string> GetGameCoinsContactsAsync(ulong index)
        {
            var function = GetFunctionGameCoinsContacts();
            return function.CallAsync<string>(index);
        }

        public Task<ulong> GetGameCoinsContactsLengthAsync()
        {
            var function = GetFunctionGameCoinsContactsLength();
            return function.CallAsync<ulong>();
        }
    }
}
