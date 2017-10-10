using Nethereum.Contracts;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.BC.Contracts
{
    class GameContract
    {
        public static string ABI = @"[{'constant':true,'inputs':[{'name':'adr','type':'address'},{'name':'assetId','type':'uint64'}],'name':'balanceOf','outputs':[{'name':'balance','type':'uint64'}],'payable':false,'type':'function'},{'constant':true,'inputs':[{'name':'assetId','type':'uint64'}],'name':'totalBalanceOf','outputs':[{'name':'','type':'uint64'}],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'receiver','type':'address'},{'name':'assetId','type':'uint64'},{'name':'amount','type':'uint64'}],'name':'transfer','outputs':[{'name':'','type':'bool'}],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'from','type':'address'},{'name':'to','type':'address'},{'name':'assetId','type':'uint64'},{'name':'amount','type':'uint64'}],'name':'gameOwnerTransfer','outputs':[{'name':'','type':'bool'}],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'adr','type':'address'},{'name':'assetId','type':'uint64'},{'name':'amount','type':'uint64'}],'name':'addGameAsset','outputs':[{'name':'','type':'bool'}],'payable':false,'type':'function'},{'constant':true,'inputs':[],'name':'gameOwner','outputs':[{'name':'','type':'address'}],'payable':false,'type':'function'},{'constant':false,'inputs':[{'name':'adr','type':'address'},{'name':'assetId','type':'uint64'},{'name':'amount','type':'uint64'}],'name':'burn','outputs':[{'name':'','type':'bool'}],'payable':false,'type':'function'},{'inputs':[{'name':'_gameOwner','type':'address'},{'name':'_gameId','type':'uint64'}],'payable':false,'type':'constructor'}]";

        private readonly Web3 web3;
        private Contract contract;

        public GameContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }
    }
}
