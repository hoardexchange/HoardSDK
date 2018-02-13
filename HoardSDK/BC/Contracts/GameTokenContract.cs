using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Contracts;

namespace Hoard.BC.Contracts
{
    public abstract class GameTokenContract
    {
        private readonly Web3 web3;
        private Contract contract;

        protected abstract string ABI { get; }

        public string Address { get { return contract.Address; } }

        public GameTokenContract(Web3 web3, string address)
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

        private Function GetFunctionName()
        {
            return contract.GetFunction("name");
        }

        private Function GetFunctionTotalSupply()
        {
            return contract.GetFunction("totalSupply");
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

        public Task<string> Name()
        {
            var function = GetFunctionName();
            return function.CallAsync<string>();
        }

        public Task<ulong> TotalSupply()
        {
            var function = GetFunctionTotalSupply();
            return function.CallAsync<ulong>();
        }
    }
}
