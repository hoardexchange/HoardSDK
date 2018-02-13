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
        public static string ABI = @"";

        private readonly Web3 web3;
        private Contract contract;

        public GameExchangeContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        private Function GetFunctionTrade()
        {
            return contract.GetFunction("trade");
        }

        public Task<object> Trade(
            string tokenGet, 
            ulong amountGet, 
            string tokenGive,
            ulong amountGive,
            ulong expires,
            ulong nonce,
            ulong amount)
        {
            var function = GetFunctionTrade();
            return function.CallAsync<object>(
                tokenGet, 
                amountGet, 
                tokenGive, 
                amountGive, 
                expires, 
                nonce, 
                amount);
        }

    }
}
