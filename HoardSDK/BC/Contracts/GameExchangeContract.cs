using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Contracts;

namespace Hoard.BC.Contracts
{
    //TODO: comment it!
    public class GameExchangeContract
    {
        public const string ABI = HoardABIConfig.HoardExchangeABI;

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
