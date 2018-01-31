using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Contracts;

namespace Hoard.BC.Contracts
{
    public class GameCoinContract
    {
        public static string ABI = @"[ { 'constant': true, 'inputs': [], 'name': 'name', 'outputs': [ { 'name': '', 'type': 'string' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'totalSupply', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'decimals', 'outputs': [ { 'name': '', 'type': 'uint8' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'MAX_UINT256', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '_owner', 'type': 'address' } ], 'name': 'balanceOf', 'outputs': [ { 'name': 'balance', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'owner', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'symbol', 'outputs': [ { 'name': '', 'type': 'string' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_to', 'type': 'address' }, { 'name': '_value', 'type': 'uint256' } ], 'name': 'transfer', 'outputs': [ { 'name': 'success', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_to', 'type': 'address' }, { 'name': '_value', 'type': 'uint256' }, { 'name': '_data', 'type': 'bytes' } ], 'name': 'transfer', 'outputs': [ { 'name': 'success', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_to', 'type': 'address' }, { 'name': '_value', 'type': 'uint256' }, { 'name': '_data', 'type': 'bytes' }, { 'name': '_custom_fallback', 'type': 'string' } ], 'name': 'transfer', 'outputs': [ { 'name': 'success', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'inputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'constructor' }, { 'anonymous': false, 'inputs': [ { 'indexed': true, 'name': 'from', 'type': 'address' }, { 'indexed': true, 'name': 'to', 'type': 'address' }, { 'indexed': false, 'name': 'value', 'type': 'uint256' }, { 'indexed': false, 'name': 'data', 'type': 'bytes' } ], 'name': 'Transfer', 'type': 'event' } ]";

        private readonly Web3 web3;
        private Contract contract;

        public GameCoinContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        private Function GetFunctionBalanceOf()
        {
            return contract.GetFunction("balanceOf");
        }

        private Function GetFunctionSymbol()
        {
            return contract.GetFunction("symbol");
        }

        public Task<ulong> BalanceOf(string address)
        {
            var function = GetFunctionBalanceOf();
            return function.CallAsync<ulong>(address);
        }

        public Task<string> Symbol()
        {
            var function = GetFunctionSymbol();
            return function.CallAsync<string>();
        }

    }
}
