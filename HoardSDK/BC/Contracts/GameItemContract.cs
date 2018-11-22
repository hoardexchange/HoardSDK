using Nethereum.Contracts;
using Nethereum.Web3;
using System;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;

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

        public const string ABI = HoardABIConfig.SupportsInterfaceWithLookupABI;

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

    /// <summary>
    /// Base Hoard Game Item contract
    /// </summary>
    public abstract class GameItemContract
    {
        protected GameID Game { get; private set; }
        protected readonly Web3 web3;
        protected Contract contract;

        public string Address { get { return contract.Address; } }

        public GameItemContract(GameID game, Web3 web3, string address, string abi)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(abi, address);
            Game = game;
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

        private Function GetFunctionItemType()
        {
            return contract.GetFunction("tokenType");
        }

        private Function GetFunctionTokenStateType()
        {
            return contract.GetFunction("tokenStateType");
        }

        public Task<BigInteger> GetBalanceOf(string address)
        {
            var function = GetFunctionBalanceOf();
            return function.CallAsync<BigInteger>(address);
        }

        public Task<string> GetSymbol()
        {
            var function = GetFunctionSymbol();
            return function.CallAsync<string>();
        }

        public Task<string> GetName()
        {
            var function = GetFunctionName();
            return function.CallAsync<string>();
        }

        // FIXME: should be BigInteger
        public Task<ulong> GetTotalSupply()
        {
            var function = GetFunctionTotalSupply();
            return function.CallAsync<ulong>();
        }

        public Task<string> GetItemType()
        {
            var function = GetFunctionItemType();
            return function.CallAsync<string>();
        }

        public Task<byte[]> GetTokenStateType()
        {
            var function = GetFunctionTokenStateType();
            return function.CallAsync<byte[]>();
        }

        public abstract Task<bool> Transfer(string addressFrom, string addressTo, GameItem item, ulong amount);

        public abstract Task<GameItem[]> GetGameItems(AccountInfo info);

        public abstract Task<GameItem[]> GetGameItems(GameItemsParams gameItemsParams);
    }

    /// <summary>
    /// ERC223 Game Item contract.
    /// </summary>
    public class ERC223GameItemContract : GameItemContract
    {
        public class Metadata : BaseGameItemMetadata
        {
            public string State { get; set; }
            public string OwnerAddress { get; set; }
            public BigInteger Balance { get; set; }

            public Metadata(string state, string ownerAddress, BigInteger balance)
            {
                State = state;
                OwnerAddress = ownerAddress;
                Balance = balance;
            }
        }

        public const string InterfaceID = "0x5713b3c1";

        public const string ABI = HoardABIConfig.ERC223TokenABI;

        public ERC223GameItemContract(GameID game, Web3 web3, string address) : base(game, web3, address, ABI) { }
        public ERC223GameItemContract(GameID game, Web3 web3, string address, string abi) : base(game, web3, address, abi) { }

        private Function GetFunctionTransfer()
        {
            return contract.GetFunction("transfer");
        }

        private Function GetFunctionTokenState()
        {
            return contract.GetFunction("tokenState");
        }

        public Task<byte[]> GetTokenState()
        {
            var function = GetFunctionTokenState();
            return function.CallAsync<byte[]>();
        }

        public override async Task<bool> Transfer(string addressFrom, string addressTo, GameItem item, ulong amount)
        {
            var function = GetFunctionTransfer();
            var gas = await function.EstimateGasAsync(addressFrom, new Nethereum.Hex.HexTypes.HexBigInteger(100000), new Nethereum.Hex.HexTypes.HexBigInteger(0), addressTo, amount);
            gas = new Nethereum.Hex.HexTypes.HexBigInteger(gas.Value * 2);
            var receipt = await function.SendTransactionAndWaitForReceiptAsync(addressFrom, gas, new Nethereum.Hex.HexTypes.HexBigInteger(0), null, addressTo, amount);
            return receipt.Status.Value == 1;
        }

        public override async Task<GameItem[]> GetGameItems(AccountInfo info)
        {
            BigInteger itemBalance = await GetBalanceOf(info.ID);
            if (BigInteger.Zero.CompareTo(itemBalance)<0)
            {
                string state = BitConverter.ToString(await GetTokenState());
                Metadata meta = new Metadata(state, Address, itemBalance);
                GameItem gi = new GameItem(Game, await GetSymbol(), meta);
                return new GameItem[] { gi };
            }
            else
                return new GameItem[0];
        }

        public override async Task<GameItem[]> GetGameItems(GameItemsParams gameItemsParams)
        {
            BigInteger itemBalance = await GetBalanceOf(gameItemsParams.PlayerAddress);
            string state = BitConverter.ToString(await GetTokenState());
            Metadata meta = new Metadata(state, Address, itemBalance);
            GameItem gi = new GameItem(Game, await GetSymbol(), meta);
            return new GameItem[] { gi };
        }
    }

    /// <summary>
    /// ERC721 Game Item contract
    /// </summary>
    public class ERC721GameItemContract : GameItemContract
    {
        public class Metadata : BaseGameItemMetadata
        {
            public string OwnerAddress { get; set; }
            public BigInteger ItemId { get; set; }

            public Metadata(string ownerAddress, BigInteger itemID)
            {
                OwnerAddress = ownerAddress;
                ItemId = itemID;
            }
        }

        public const string InterfaceID = "0x80ac58cd";

        public const string ABI = HoardABIConfig.ERC721TokenABI;

        public ERC721GameItemContract(GameID game, Web3 web3, string address) : base(game, web3, address, ABI) { }
        public ERC721GameItemContract(GameID game, Web3 web3, string address, string abi) : base(game, web3, address, abi) { }

        private Function GetFunctionTransfer()
        {
            return contract.GetFunction("safeTransferFrom");
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

        // FIXME: itemID should be BigInteger
        public Task<byte[]> GetTokenState(BigInteger itemID)
        {
            Function function = GetFunctionTokenState();
            return function.CallAsync<byte[]>(itemID);
        }

        public Task<bool> Exists(BigInteger itemID)
        {
            Function function = GetFunctionExists();
            return function.CallAsync<bool>(itemID);
        }

        // FIXME: should be BigInteger
        public Task<BigInteger> TokenOfOwnerByIndex(string owner, ulong index)
        {
            Function function = GetFunctionTokenOfOwnerByIndex();
            return function.CallAsync<BigInteger>(owner, index);
        }

        public Task<BigInteger> OwnerOf(BigInteger index)
        {
            Function function = contract.GetFunction("ownerOf");
            return function.CallAsync<BigInteger>(index);
        }

        public override async Task<bool> Transfer(string addressFrom, string addressTo, GameItem item, ulong amount)
        {
            var function = GetFunctionTransfer();
            BigInteger tokenId = (item.Metadata as ERC721GameItemContract.Metadata).ItemId;
            var gas = await function.EstimateGasAsync(addressFrom, new Nethereum.Hex.HexTypes.HexBigInteger(500000), new Nethereum.Hex.HexTypes.HexBigInteger(0), addressFrom, addressTo, tokenId);
            gas = new Nethereum.Hex.HexTypes.HexBigInteger(gas.Value * 2);
            var receipt = await function.SendTransactionAndWaitForReceiptAsync(addressFrom, gas, new Nethereum.Hex.HexTypes.HexBigInteger(0), null, addressFrom, addressTo, tokenId);
            return receipt.Status.Value == 1;
        }

        public override async Task<GameItem[]> GetGameItems(AccountInfo info)
        {
            BigInteger itemBalance = await GetBalanceOf(info.ID);
            string symbol = await GetSymbol();

            ulong count = (ulong)itemBalance;

            GameItem[] items = new GameItem[count];

            for (ulong i = 0; i < count; ++i)
            {
                BigInteger id = await TokenOfOwnerByIndex(info.ID, i);
                Metadata meta = new Metadata(Address, id);

                items[i] = new GameItem(Game, symbol, meta);
                items[i].State = await GetTokenState(id);
            }

            return items;
        }

        public override async Task<GameItem[]> GetGameItems(GameItemsParams gameItemsParams)
        {
            BigInteger itemBalance = await GetBalanceOf(gameItemsParams.PlayerAddress);
            string symbol = await GetSymbol();

            ulong count = (ulong)itemBalance;

            GameItem[] items = new GameItem[1];

            BigInteger id = BigInteger.Parse(gameItemsParams.TokenId, NumberStyles.AllowHexSpecifier);
            Metadata meta = new Metadata(Address, id);

            items[0] = new GameItem(Game, symbol, meta);
            items[0].State = await GetTokenState(id);

            return items;
        }
    }
}
