using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Contracts;

namespace Hoard.BC.Contracts
{
    class GameExchangeContract
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
            uint amountGet, 
            string tokenGive, 
            uint amountGive, 
            uint expires, 
            uint nonce, 
            uint amount)
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
