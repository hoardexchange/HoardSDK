using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Contracts;

namespace Hoard.BC.Contracts
{
    public class GameExchangeContract
    {
        public static string ABI = @" [ { 'constant': false, 'inputs': [ { 'name': 'tokenGet', 'type': 'address' }, { 'name': 'amountGet', 'type': 'uint256' }, { 'name': 'tokenGive', 'type': 'address' }, { 'name': 'amountGive', 'type': 'uint256' }, { 'name': 'expires', 'type': 'uint256' }, { 'name': 'nonce', 'type': 'uint256' } ], 'name': 'order', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'address' }, { 'name': '', 'type': 'bytes32' } ], 'name': 'orderFills', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': 'tokenGet', 'type': 'address' }, { 'name': 'amountGet', 'type': 'uint256' }, { 'name': 'tokenGive', 'type': 'address' }, { 'name': 'amountGive', 'type': 'uint256' }, { 'name': 'expires', 'type': 'uint256' }, { 'name': 'nonce', 'type': 'uint256' }, { 'name': 'user', 'type': 'address' }, { 'name': 'amount', 'type': 'uint256' }, { 'name': 'sender', 'type': 'address' } ], 'name': 'testTrade', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'address' } ], 'name': 'allowedContacts', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': 'tokenGet', 'type': 'address' }, { 'name': 'amountGet', 'type': 'uint256' }, { 'name': 'tokenGive', 'type': 'address' }, { 'name': 'amountGive', 'type': 'uint256' }, { 'name': 'expires', 'type': 'uint256' }, { 'name': 'nonce', 'type': 'uint256' }, { 'name': 'user', 'type': 'address' } ], 'name': 'amountFilled', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'address' }, { 'name': '', 'type': 'address' } ], 'name': 'tokens', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': 'tokenGet', 'type': 'address' }, { 'name': 'amountGet', 'type': 'uint256' }, { 'name': 'tokenGive', 'type': 'address' }, { 'name': 'amountGive', 'type': 'uint256' }, { 'name': 'expires', 'type': 'uint256' }, { 'name': 'nonce', 'type': 'uint256' }, { 'name': 'user', 'type': 'address' } ], 'name': 'availableVolume', 'outputs': [ { 'name': '', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'tokenGet', 'type': 'address' }, { 'name': 'amountGet', 'type': 'uint256' }, { 'name': 'tokenGive', 'type': 'address' }, { 'name': 'amountGive', 'type': 'uint256' }, { 'name': 'expires', 'type': 'uint256' }, { 'name': 'nonce', 'type': 'uint256' } ], 'name': 'cancelOrder', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'owner', 'outputs': [ { 'name': '', 'type': 'address' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'tokenGet', 'type': 'address' }, { 'name': 'amountGet', 'type': 'uint256' }, { 'name': 'tokenGive', 'type': 'address' }, { 'name': 'amountGive', 'type': 'uint256' }, { 'name': 'expires', 'type': 'uint256' }, { 'name': 'nonce', 'type': 'uint256' }, { 'name': 'user', 'type': 'address' }, { 'name': 'amount', 'type': 'uint256' } ], 'name': 'trade', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'contractAddress', 'type': 'address' } ], 'name': 'deny', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_token', 'type': 'address' }, { 'name': '_value', 'type': 'uint256' } ], 'name': 'withdrawToken', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '', 'type': 'address' }, { 'name': '', 'type': 'bytes32' } ], 'name': 'orders', 'outputs': [ { 'name': '', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_from', 'type': 'address' }, { 'name': '_value', 'type': 'uint256' }, { 'name': '', 'type': 'bytes' } ], 'name': 'tokenFallback', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'newOwner', 'type': 'address' } ], 'name': 'transferOwnership', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': 'contractAddress', 'type': 'address' } ], 'name': 'allow', 'outputs': [], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'payable': false, 'stateMutability': 'nonpayable', 'type': 'fallback' }, { 'anonymous': false, 'inputs': [ { 'indexed': true, 'name': 'from', 'type': 'address' }, { 'indexed': true, 'name': 'token', 'type': 'address' }, { 'indexed': false, 'name': 'value', 'type': 'uint256' } ], 'name': 'Deposit', 'type': 'event' }, { 'anonymous': false, 'inputs': [ { 'indexed': true, 'name': 'to', 'type': 'address' }, { 'indexed': true, 'name': 'token', 'type': 'address' }, { 'indexed': false, 'name': 'value', 'type': 'uint256' } ], 'name': 'Withdraw', 'type': 'event' }, { 'anonymous': false, 'inputs': [ { 'indexed': true, 'name': 'tokenGet', 'type': 'address' }, { 'indexed': false, 'name': 'amountGet', 'type': 'uint256' }, { 'indexed': true, 'name': 'tokenGive', 'type': 'address' }, { 'indexed': false, 'name': 'amountGive', 'type': 'uint256' }, { 'indexed': false, 'name': 'expires', 'type': 'uint256' }, { 'indexed': false, 'name': 'nonce', 'type': 'uint256' }, { 'indexed': true, 'name': 'from', 'type': 'address' } ], 'name': 'Order', 'type': 'event' }, { 'anonymous': false, 'inputs': [ { 'indexed': true, 'name': 'tokenGet', 'type': 'address' }, { 'indexed': false, 'name': 'amountGet', 'type': 'uint256' }, { 'indexed': true, 'name': 'tokenGive', 'type': 'address' }, { 'indexed': false, 'name': 'amountGive', 'type': 'uint256' }, { 'indexed': false, 'name': 'expires', 'type': 'uint256' }, { 'indexed': false, 'name': 'nonce', 'type': 'uint256' }, { 'indexed': false, 'name': 'get', 'type': 'uint256' }, { 'indexed': false, 'name': 'give', 'type': 'uint256' }, { 'indexed': false, 'name': 'getAddress', 'type': 'address' }, { 'indexed': false, 'name': 'giveAddress', 'type': 'address' } ], 'name': 'Trade', 'type': 'event' }, { 'anonymous': false, 'inputs': [ { 'indexed': true, 'name': 'tokenGet', 'type': 'address' }, { 'indexed': false, 'name': 'amountGet', 'type': 'uint256' }, { 'indexed': true, 'name': 'tokenGive', 'type': 'address' }, { 'indexed': false, 'name': 'amountGive', 'type': 'uint256' }, { 'indexed': false, 'name': 'expires', 'type': 'uint256' }, { 'indexed': false, 'name': 'nonce', 'type': 'uint256' }, { 'indexed': true, 'name': 'user', 'type': 'address' } ], 'name': 'Cancel', 'type': 'event' }, { 'anonymous': false, 'inputs': [ { 'indexed': true, 'name': 'previousOwner', 'type': 'address' }, { 'indexed': true, 'name': 'newOwner', 'type': 'address' } ], 'name': 'OwnershipTransferred', 'type': 'event' } ]";

        private readonly Web3 web3;
        private Contract contract;

        public string Address { get { return contract.Address; } }

        public GameExchangeContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        private Function GetFunctionTrade()
        {
            return contract.GetFunction("trade");
        }

        private Function GetFunctionTestTrade()
        {
            return contract.GetFunction("testTrade");
        }

        private Function GetFunctionWithdrawToken()
        {
            return contract.GetFunction("withdrawToken");
        }

        public async Task<bool> Trade(
            string tokenGet, 
            ulong amountGet, 
            string tokenGive,
            ulong amountGive,
            ulong expires,
            ulong nonce,
            string user,
            ulong amount,
            string from)
        {
            var testTradeFun = GetFunctionTestTrade();

            var test = await testTradeFun.CallAsync<bool>(
                tokenGet,
                amountGet,
                tokenGive,
                amountGive,
                expires,
                nonce,
                user,
                amount,
                from);

            if (!test)
                return false;

            var function = GetFunctionTrade();
            var gas = await function.EstimateGasAsync(
                from,
                new Nethereum.Hex.HexTypes.HexBigInteger(1000000),
                new Nethereum.Hex.HexTypes.HexBigInteger(0),
                tokenGet,
                amountGet,
                tokenGive,
                amountGive,
                expires,
                nonce,
                user,
                amount);

            gas = new Nethereum.Hex.HexTypes.HexBigInteger(gas.Value * 2);
            var receipt = await function.SendTransactionAndWaitForReceiptAsync(
                from,
                gas,
                new Nethereum.Hex.HexTypes.HexBigInteger(0),
                null,
                tokenGet, 
                amountGet, 
                tokenGive, 
                amountGive, 
                expires, 
                nonce,
                user,
                amount);
            return receipt.Status.Value == 1;
        }

        public async Task<bool> Withdraw(string tokenAddress, ulong value, string from)
        {
            var function = GetFunctionWithdrawToken();
            var gas = await function.EstimateGasAsync(from, new Nethereum.Hex.HexTypes.HexBigInteger(100000), new Nethereum.Hex.HexTypes.HexBigInteger(0), tokenAddress, value);
            gas = new Nethereum.Hex.HexTypes.HexBigInteger(gas.Value * 2);
            var receipt = await function.SendTransactionAndWaitForReceiptAsync(from, gas, new Nethereum.Hex.HexTypes.HexBigInteger(0), null, tokenAddress, value);
            return receipt.Status.Value == 1;
        }
    }
}
