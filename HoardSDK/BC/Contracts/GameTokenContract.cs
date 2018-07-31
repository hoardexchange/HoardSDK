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
    public abstract class GameItemContract
    {
        protected readonly Web3 web3;
        protected Contract contract;

        public string Address { get { return contract.Address; } }

        public GameItemContract(Web3 web3, string address, string abi)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(abi, address);

            // TODO: remove this test code
            //string abi2 = "[{ 'constant':false,'inputs':[{'name':'_a','type':'uint256'},{'name':'_b','type':'uint256'}],'name':'multiply','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':false,'inputs':[{'name':'_a','type':'uint256'},{'name':'_b','type':'uint256'}],'name':'arithmetics','outputs':[{'name':'o_sum','type':'uint256'},{'name':'o_product','type':'uint256'}],'payable':false,'stateMutability':'nonpayable','type':'function'},{'constant':true,'inputs':[],'name':'prop_b','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'},{'constant':true,'inputs':[],'name':'prop_a','outputs':[{'name':'','type':'uint256'}],'payable':false,'stateMutability':'view','type':'function'}]";
            //this.contract = web3.Eth.GetContract(abi2, address);
            //FillProperties(null);
            //
        }

        //public void FillProperties(GameAsset asset)
        //{
        //    for (int i = 0; i < contract.ContractBuilder.ContractABI.Functions.Length; i++)
        //    {
        //        Nethereum.ABI.Model.FunctionABI fabi = contract.ContractBuilder.ContractABI.Functions[i];
        //        if (fabi.Name.Contains("prop_"))
        //        {
        //            Prop prop;
        //            if (asset.Properties.Properties.TryGetValue(fabi.Name, out prop) == false)
        //                continue;
        //            Function fun = contract.GetFunction(fabi.Name);
        //            switch (prop.type)
        //            {
        //                case PropertyType.String:
        //                case PropertyType.Address:
        //                    var retString = fun.CallAsync<string>();
        //                    asset.Properties.Set(fabi.Name, retString.Result);
        //                    break;
        //                case PropertyType.Bool:
        //                    var retBool = fun.CallAsync<bool>();
        //                    asset.Properties.Set(fabi.Name, retBool.Result);
        //                    break;
        //                case PropertyType.Int16:
        //                    var retInt16 = fun.CallAsync<short>();
        //                    asset.Properties.Set(fabi.Name, retInt16.Result);
        //                    break;
        //                case PropertyType.Int32:
        //                    var retInt32 = fun.CallAsync<int>();
        //                    asset.Properties.Set(fabi.Name, retInt32.Result);
        //                    break;
        //                case PropertyType.Int64:
        //                    var retInt64 = fun.CallAsync<long>();
        //                    asset.Properties.Set(fabi.Name, retInt64.Result);
        //                    break;
        //                case PropertyType.Uint16:
        //                    var retUint16 = fun.CallAsync<ushort>();
        //                    asset.Properties.Set(fabi.Name, retUint16.Result);
        //                    break;
        //                case PropertyType.Uint32:
        //                    var retUint32 = fun.CallAsync<uint>();
        //                    asset.Properties.Set(fabi.Name, retUint32.Result);
        //                    break;
        //                case PropertyType.Uint64:
        //                    var retUint64 = fun.CallAsync<ulong>();
        //                    asset.Properties.Set(fabi.Name, retUint64.Result);
        //                    break;
        //                case PropertyType.BigInt:
        //                    var retBigInteger = fun.CallAsync<BigInteger>();
        //                    asset.Properties.Set(fabi.Name, retBigInteger.Result);
        //                    break;
        //                default:
        //                    Debug.Assert(false, "Unknown property type!");
        //                    break;
        //            }
        //        }
        //    }
        //}

        //public async Task<bool> SavePropertyToBC(BCComm comm, GameAsset asset, string propertyName)
        //{
        //    Prop prop;
        //    if (asset.Properties.Properties.TryGetValue(propertyName, out prop) == false)
        //        return await Task.FromResult<bool>(false);
        //    Function function = contract.GetFunction("set" + propertyName);
        //    return await comm.EvaluateOnBC((address) =>
        //    {
        //        return function.SendTransactionAsync(address, new HexBigInteger(4700000), new HexBigInteger(0), prop.value);
        //    });
        //}

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

        private Function GetFunctionChecksum()
        {
            return contract.GetFunction("checksum");
        }

        private Function GetFunctionPropertyType()
        {
            return contract.GetFunction("propertyType");
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

        public Task<string> AssetType()
        {
            var function = GetFunctionAssetType();
            return function.CallAsync<string>();
        }

        public Task<ulong> Checksum()
        {
            var function = GetFunctionChecksum();
            return function.CallAsync<ulong>();
        }

        public Task<ulong> PropertyType()
        {
            var function = GetFunctionPropertyType();
            return function.CallAsync<ulong>();
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
            public ulong Checksum { get; set; }
            public string OwnerAddress { get; set; }
            public ulong Balance { get; set; }

            public Metadata(ulong checksum, string ownerAddress, ulong balance)
            {
                Checksum = checksum;
                OwnerAddress = ownerAddress;
                Balance = balance;
            }
        }

        public static string ABI = @"[ { 'constant': true, 'inputs': [], 'name': 'name', 'outputs': [ { 'name': '_name', 'type': 'string' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'totalSupply', 'outputs': [ { 'name': '_totalSupply', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'tokenType', 'outputs': [ { 'name': '', 'type': 'bytes32' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'decimals', 'outputs': [ { 'name': '_decimals', 'type': 'uint8' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [ { 'name': '_owner', 'type': 'address' } ], 'name': 'balanceOf', 'outputs': [ { 'name': 'balance', 'type': 'uint256' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': true, 'inputs': [], 'name': 'symbol', 'outputs': [ { 'name': '_symbol', 'type': 'string' } ], 'payable': false, 'stateMutability': 'view', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_to', 'type': 'address' }, { 'name': '_value', 'type': 'uint256' } ], 'name': 'transfer', 'outputs': [ { 'name': 'success', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_to', 'type': 'address' }, { 'name': '_value', 'type': 'uint256' }, { 'name': '_data', 'type': 'bytes' } ], 'name': 'transfer', 'outputs': [ { 'name': 'success', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'constant': false, 'inputs': [ { 'name': '_to', 'type': 'address' }, { 'name': '_value', 'type': 'uint256' }, { 'name': '_data', 'type': 'bytes' }, { 'name': '_custom_fallback', 'type': 'string' } ], 'name': 'transfer', 'outputs': [ { 'name': 'success', 'type': 'bool' } ], 'payable': false, 'stateMutability': 'nonpayable', 'type': 'function' }, { 'anonymous': false, 'inputs': [ { 'indexed': true, 'name': 'from', 'type': 'address' }, { 'indexed': true, 'name': 'to', 'type': 'address' }, { 'indexed': false, 'name': 'value', 'type': 'uint256' }, { 'indexed': false, 'name': 'data', 'type': 'bytes' } ], 'name': 'Transfer', 'type': 'event' } ]";

        public ERC223GameItemContract(Web3 web3, string address) : base(web3, address, ABI) { }

        public override async Task<GameItem[]> GetGameItems(PlayerID playerID)
        {
            Metadata meta = new Metadata(await Checksum(), Address, await BalanceOf(playerID.ID));
            GameItem gi = new GameItem(await Symbol(), meta);
            return new GameItem[] { gi };
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

            public Metadata(string ownerAddress, ulong itemID)
            {
                OwnerAddress = ownerAddress;
                ItemId = itemID;
            }
        }

        // FIXME: add abi and extend hoarditem/721 contract
        public static string ABI = "";

        public ERC721GameItemContract(Web3 web3, string address) : base(web3, address, ABI) { }

        private Function GetFunctionGetItems()
        {
            return contract.GetFunction("getItems");
        }

        private Function GetFunctionGetItemChecksum()
        {
            return contract.GetFunction("getItemChecksum");
        }

        public Task<ulong[]> GetItems(string owner, ulong startIndex, ulong numItems)
        {
            Function function = GetFunctionGetItems();
            return function.CallAsync<ulong[]>(owner, startIndex, numItems);
        }

        public Task<string> GetItemChecksum(ulong itemID)
        {
            Function function = GetFunctionGetItemChecksum();
            return function.CallAsync<string>(itemID);
        }

        protected override object[] GetTransferInput(GameItem item)
        {
            return new object[] { (item.Metadata as Metadata).ItemId };
        }

        public override async Task<GameItem[]> GetGameItems(PlayerID playerID)
        {
            ulong[] ids = await GetItems(playerID.ID, 0, await BalanceOf(playerID.ID));
            string symbol = await Symbol();

            GameItem[] items = new GameItem[ids.Length];

            foreach (ulong id in ids)
            {
                Metadata meta = new Metadata(Address, id);
                GameItem gi = new GameItem(symbol, meta);
                gi.Checksum = await GetItemChecksum(id);
            }
            return items;
        }
    }
}
