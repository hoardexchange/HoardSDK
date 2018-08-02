using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Contracts;
using System.Diagnostics;
using Org.BouncyCastle.Math;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;

namespace Hoard.BC.Contracts
{
    /// <summary>
    /// ERC165
    /// </summary>
    public class SupportsInterfaceWithLookupContract
    {
        protected readonly Web3 web3;
        protected Contract contract;

        public string Address { get { return contract.Address; } }

        public static string ABI = @"[{'constant': true,'inputs': [],'name': 'InterfaceId_ERC165','outputs': [{'name': '','type': 'bytes4'}],'payable': false,'stateMutability': 'view','type': 'function'},{'inputs': [],'payable': false,'stateMutability': 'nonpayable','type': 'constructor'},{'constant': true,'inputs': [{'name': '_interfaceId','type': 'bytes4'}],'name': 'supportsInterface','outputs': [{'name': '','type': 'bool'}],'payable': false,'stateMutability': 'view','type': 'function'}]";

        public SupportsInterfaceWithLookupContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        private Function GetFunctionSupportsInterface()
        {
            return contract.GetFunction("supportsInterface");
        }

        public Task<bool> SupportsInterface(byte[] interfaceId)
        {
            var function = GetFunctionSupportsInterface();
            return function.CallAsync<bool>(interfaceId);
        }
    }

    public abstract class GameItemContract
    {
        protected readonly Web3 web3;
        protected Contract contract;

        public string Address { get { return contract.Address; } }

        public GameItemContract(Web3 web3, string address, string abi)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(abi, address);
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

        private Function GetFunctionAssetType()
        {
            return contract.GetFunction("tokenType");
        }

        private Function GetFunctionTransfer()
        {
            return contract.GetFunction("transfer");
        }

        private Function GetFunctionTokenStateType()
        {
            return contract.GetFunction("tokenStateType");
        }

        // FIXME: should be BigInteger
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

        // FIXME: should be BigInteger
        public Task<ulong> TotalSupply()
        {
            var function = GetFunctionTotalSupply();
            return function.CallAsync<ulong>();
        }

        public Task<string> AssetType()
        {
            var function = GetFunctionAssetType();
            return function.CallAsync<string>();
        }

        public Task<byte[]> TokenStateType()
        {
            var function = GetFunctionTokenStateType();
            return function.CallAsync<byte[]>();
        }

        protected abstract object[] GetTransferInput(GameItem item);

        public async Task<bool> Transfer(string from, GameItem item)
        {
            object[] functionInput = GetTransferInput(item);
            var function = GetFunctionTransfer();
            var gas = await function.EstimateGasAsync(function.CreateTransactionInput(from, new Nethereum.Hex.HexTypes.HexBigInteger(100000), new Nethereum.Hex.HexTypes.HexBigInteger(0), functionInput));
            gas = new Nethereum.Hex.HexTypes.HexBigInteger(gas.Value * 2);
            var receipt = await function.SendTransactionAndWaitForReceiptAsync(function.CreateTransactionInput(from, gas, new Nethereum.Hex.HexTypes.HexBigInteger(0), null, functionInput));
            return receipt.Status.Value == 1;
        }

        public abstract Task<GameItem[]> GetGameItems(PlayerID playerID);
    }

    public class ERC223GameItemContract : GameItemContract
    {
        public class Metadata : BaseGameItemMetadata
        {
            public string State { get; set; }
            public string OwnerAddress { get; set; }
            public ulong Balance { get; set; }

            public Metadata(string state, string ownerAddress, ulong balance)
            {
                State = state;
                OwnerAddress = ownerAddress;
                Balance = balance;
            }
        }

        public static string ABI = @"[{'constant':true,'inputs':[{'name':'_interfaceId','type':'bytes4'}],'name':'supportsInterface','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'InterfaceId_ERC165','outputs':[{'name':'','type':'bytes4'}],'payable':false,'stateMutability':'view','type':'function'},{'inputs':[{'name':'_name','type':'string'},{'name':'_symbol','type':'string'}],'payable':false,'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'name':'from','type':'address'},{'indexed':true,'name':'to','type':'address'},{'indexed':false,'name':'value','type':'uint256'},{'indexed':false,'name':'data','type':'bytes'}],'name':'Transfer','type':'event'},{'constant':true,'inputs':[],'name':'name','outputs':[{'name':'_name','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'symbol','outputs':[{'name':'_symbol','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'decimals','outputs':[{'name':'_decimals','type':'uint8'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'totalSupply','outputs':[{'name':'_totalSupply','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_value','type':'uint256'}],'name':'transfer','outputs':[{'name':'success','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_value','type':'uint256'},{'name':'_data','type':'bytes'}],'name':'transfer','outputs':[{'name':'success','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_value','type':'uint256'},{'name':'_data','type':'bytes'},{'name':'_custom_fallback','type':'string'}],'name':'transfer','outputs':[{'name':'success','type':'bool'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'_owner','type':'address'}],'name':'balanceOf','outputs':[{'name':'balance','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'}]";

        public ERC223GameItemContract(Web3 web3, string address) : base(web3, address, ABI) { }

        private Function GetFunctionTokenState()
        {
            return contract.GetFunction("tokenState");
        }

        public Task<byte[]> TokenState()
        {
            var function = GetFunctionTokenState();
            return function.CallAsync<byte[]>();
        }

        public override async Task<GameItem[]> GetGameItems(PlayerID playerID)
        {
            ulong itemBalance = await BalanceOf(playerID.ID);
            if (itemBalance > 0)
            {
                string state = BitConverter.ToString(await TokenState());
                Metadata meta = new Metadata(state, Address, itemBalance);
                GameItem gi = new GameItem(await Symbol(), meta);
                return new GameItem[] { gi };
            }
            else
                return new GameItem[0];
        }

        protected override object[] GetTransferInput(GameItem item)
        {
            return new object[] { (item.Metadata as Metadata).Balance };
        }
    }

    public class ERC721GameItemContract : GameItemContract
    {
        public class Metadata : BaseGameItemMetadata
        {            
            public string OwnerAddress { get; set; }
            public ulong ItemId { get; set; }
            public string TokenStateType { get; set; }

            public Metadata(string ownerAddress, ulong itemID, string tokenStateType)
            {
                OwnerAddress = ownerAddress;
                ItemId = itemID;
                TokenStateType = tokenStateType;
            }
        }

        public static string ABI = @"[{'constant':true,'inputs':[{'name':'_interfaceId','type':'bytes4'}],'name':'supportsInterface','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_tokenId','type':'uint256'}],'name':'getApproved','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_tokenId','type':'uint256'}],'name':'approve','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'InterfaceId_ERC165','outputs':[{'name':'','type':'bytes4'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_to','type':'address'},{'name':'_tokenId','type':'uint256'}],'name':'transferFrom','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_to','type':'address'},{'name':'_tokenId','type':'uint256'}],'name':'safeTransferFrom','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'_tokenId','type':'uint256'}],'name':'exists','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_tokenId','type':'uint256'}],'name':'ownerOf','outputs':[{'name':'','type':'address'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_owner','type':'address'}],'name':'balanceOf','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':false,'inputs':[{'name':'_to','type':'address'},{'name':'_approved','type':'bool'}],'name':'setApprovalForAll','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_from','type':'address'},{'name':'_to','type':'address'},{'name':'_tokenId','type':'uint256'},{'name':'_data','type':'bytes'}],'name':'safeTransferFrom','outputs':[],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[{'name':'_owner','type':'address'},{'name':'_operator','type':'address'}],'name':'isApprovedForAll','outputs':[{'name':'','type':'bool'}],'payable':false,'stateMutability':'view','type':'function'},{'inputs':[{'name':'_name','type':'string'},{'name':'_symbol','type':'string'}],'payable':false,'stateMutability':'nonpayable','type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_from','type':'address'},{'indexed':true,'name':'_to','type':'address'},{'indexed':true,'name':'_tokenId','type':'uint256'}],'name':'Transfer','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_owner','type':'address'},{'indexed':true,'name':'_approved','type':'address'},{'indexed':true,'name':'_tokenId','type':'uint256'}],'name':'Approval','type':'event'},{'anonymous':false,'inputs':[{'indexed':true,'name':'_owner','type':'address'},{'indexed':true,'name':'_operator','type':'address'},{'indexed':false,'name':'_approved','type':'bool'}],'name':'ApprovalForAll','type':'event'},{'constant':true,'inputs':[],'name':'name','outputs':[{'name':'','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'symbol','outputs':[{'name':'','type':'string'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_tokenId','type':'uint256'}],'name':'tokenState','outputs':[{'name':'','type':'bytes32'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_owner','type':'address'},{'name':'_index','type':'uint256'}],'name':'tokenOfOwnerByIndex','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'totalSupply','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[{'name':'_index','type':'uint256'}],'name':'tokenByIndex','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'}]";

        public ERC721GameItemContract(Web3 web3, string address) : base(web3, address, ABI) { }

        // FIXME: not exists in erc721 contract
        private Function GetFunctionGetItems()
        {
            return contract.GetFunction("getItems");
        }

        private Function GetFunctionTokenState()
        {
            return contract.GetFunction("tokenState");
        }

        private Function GetFunctionTokenOfOwnerByIndex()
        {
            return contract.GetFunction("tokenOfOwnerByIndex");
        }

        private Function GetFunctionExists()
        {
            return contract.GetFunction("exists");
        }

        // FIXME: not exists in erc721 contract
        public Task<ulong[]> GetItems(string owner, ulong startIndex, ulong numItems)
        {
            Function function = GetFunctionGetItems();
            return function.CallAsync<ulong[]>(owner, startIndex, numItems);
        }

        // FIXME: itemID should be BigInteger
        public Task<byte[]> TokenState(ulong itemID)
        {
            Function function = GetFunctionTokenState();
            return function.CallAsync<byte[]>(itemID);
        }

        public Task<bool> TokenState(BigInteger itemID)
        {
            Function function = GetFunctionExists();
            return function.CallAsync<bool>(itemID);
        }

        public Task<BigInteger> TokenOfOwnerByIndex(string owner, BigInteger index)
        {
            Function function = GetFunctionTokenOfOwnerByIndex();
            return function.CallAsync<BigInteger>(owner, index);
        }

        protected override object[] GetTransferInput(GameItem item)
        {
            return new object[] { (item.Metadata as Metadata).ItemId };
        }

        public override async Task<GameItem[]> GetGameItems(PlayerID playerID)
        {
            ulong[] ids = await GetItems(playerID.ID, 0, await BalanceOf(playerID.ID));
            string symbol = await Symbol();
            string tokenStateType = Encoding.ASCII.GetString(await TokenStateType());

            GameItem[] items = new GameItem[ids.Length];

            foreach (ulong id in ids)
            {
                Metadata meta = new Metadata(Address, id, tokenStateType);
                GameItem gi = new GameItem(symbol, meta);
                gi.State = BitConverter.ToString(await TokenState(id));
            }
            return items;
        }
    }
}
