using Nethereum.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.BC.Contracts
{
    /// <summary>
    /// Contract that supports ERC165 interface (for dynamic interface lookup)
    /// </summary>
    public class SupportsInterfaceWithLookupContract
    {
        /// <summary>
        /// web3 access interface
        /// </summary>
        protected readonly Web3 web3;
        /// <summary>
        /// Instance of ethereum contract
        /// </summary>
        protected Contract contract;

        /// <summary>
        /// Address of this contract
        /// </summary>
        public string Address { get { return contract.Address; } }

        /// <summary>
        /// ABI of the contract
        /// </summary>
        public const string ABI = HoardABIConfig.IERC165ABI;

        /// <summary>
        /// Creates new instance of contract that supports interface lookups (ERC165)
        /// </summary>
        /// <param name="web3">web3 accessor</param>
        /// <param name="address">ethereum address of the contract</param>
        public SupportsInterfaceWithLookupContract(Web3 web3, string address)
        {
            this.web3 = web3;
            this.contract = web3.Eth.GetContract(ABI, address);
        }

        private Function GetFunctionSupportsInterface()
        {
            return contract.GetFunction("supportsInterface");
        }

        /// <summary>
        /// Checks if this contract supports given interface
        /// </summary>
        /// <param name="interfaceId">Identifier of the interface to check</param>
        /// <returns></returns>
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
        /// <summary>
        /// Game that manages this Game Item
        /// </summary>
        protected GameID Game { get; private set; }
        /// <summary>
        /// web3 access interface
        /// </summary>
        protected readonly Web3 web3;
        /// <summary>
        /// Ethereum contract
        /// </summary>
        protected Contract contract;

        /// <summary>
        /// Ethereum address of the contract
        /// </summary>
        public string Address { get { return contract.Address; } }

        /// <summary>
        /// Creates new instance of the contract representing Game Item
        /// </summary>
        /// <param name="game">Game that manages this item</param>
        /// <param name="web3">web3 accessor</param>
        /// <param name="address">ethereum address of the contract</param>
        /// <param name="abi">ABI of the contract</param>
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

        private Function GetFunctionOwner()
        {
            return contract.GetFunction("owner");
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

        private Function GetFunctionTokenStateType()
        {
            return contract.GetFunction("tokenStateType");
        }

        /// <summary>
        /// Returns total amount of items given account owns
        /// </summary>
        /// <param name="address">Account address of the owner</param>
        /// <returns></returns>
        public Task<BigInteger> GetBalanceOf(string address)
        {
            var function = GetFunctionBalanceOf();
            return function.CallAsync<BigInteger>(address);
        }

        /// <summary>
        /// Returns symbol of this item (type of the item)
        /// </summary>
        /// <returns></returns>
        public Task<string> GetSymbol()
        {
            var function = GetFunctionSymbol();
            return function.CallAsync<string>();
        }

        /// <summary>
        /// Returns owner address of the contract
        /// </summary>
        /// <returns></returns>
        public Task<string> GetOwner()
        {
            var function = GetFunctionOwner();
            return function.CallAsync<string>();
        }

        /// <summary>
        /// Returns name of this item
        /// </summary>
        /// <returns></returns>
        public Task<string> GetName()
        {
            var function = GetFunctionName();
            return function.CallAsync<string>();
        }

        /// <summary>
        /// Returns total amount of all minted items represented by this contract
        /// </summary>
        /// <returns></returns>
        public Task<BigInteger> GetTotalSupply()
        {
            var function = GetFunctionTotalSupply();
            return function.CallAsync<BigInteger>();
        }

        /// <summary>
        /// Returns type of the Item State (IPFS hash, SWARM hash, compound U256 state, etc.)
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> GetTokenStateType()
        {
            var function = GetFunctionTokenStateType();
            return function.CallAsync<byte[]>();
        }

        /// <summary>
        /// Transfers <paramref name="item"/> from account <paramref name="from"/> to <paramref name="addressTo"/>
        /// in given <paramref name="amount"/>
        /// </summary>
        /// <param name="from">Profile of the sender</param>
        /// <param name="addressTo">destination profile address</param>
        /// <param name="item">Item to transfer</param>
        /// <param name="amount">Amount of items to transfer</param>
        /// <returns></returns>
        public abstract Task<bool> Transfer(Profile from, string addressTo, GameItem item, BigInteger amount);

        /// <summary>
        /// Returns all Game Items owned by Profile <paramref name="profile"/>
        /// </summary>
        /// <param name="profile">Player profile</param>
        /// <returns></returns>
        public abstract Task<GameItem[]> GetGameItems(Profile profile);

        /// <summary>
        /// Returns all Game Items owned by Profile <paramref name="profile"/>
        /// </summary>
        /// <param name="profile">Player profile</param>
        /// <param name="page">Page number</param>
        /// <param name="itemsPerPage">Number of items per page</param>
        /// <returns></returns>
        public abstract Task<GameItem[]> GetGameItems(Profile profile, ulong page, ulong itemsPerPage);

        /// <summary>
        /// Returns specific Game Item based on query parameters
        /// </summary>
        /// <param name="gameItemsParams">query parameters</param>
        /// <returns></returns>
        public abstract Task<GameItem> GetGameItem(GameItemsParams gameItemsParams);
    }

    /// <summary>
    /// ERC223 Game Item contract.
    /// </summary>
    public class ERC223GameItemContract : GameItemContract
    {
        /// <summary>
        /// Metadata of ERC223 Game Item contract
        /// </summary>
        public class Metadata : BaseGameItemMetadata
        {
            /// <summary>
            /// Ethereum address of the owner
            /// </summary>
            public string OwnerAddress { get; set; }
            /// <summary>
            /// Total balance of this item (how many of these items owner has)
            /// </summary>
            public BigInteger Balance { get; set; }

            /// <summary>
            /// Creates new instance of metadata object
            /// </summary>
            /// <param name="ownerAddress">address of the item owner</param>
            /// <param name="balance">total amount of owned instances</param>
            public Metadata(string ownerAddress, BigInteger balance)
            {
                OwnerAddress = ownerAddress;
                Balance = balance;
            }
        }

        /// <summary>
        /// Identifier of ERC223GameItem interface
        /// </summary>
        public const string InterfaceID = "0x5713b3c1";

        /// <summary>
        /// Base ABI of the contract
        /// </summary>
        public const string ABI = HoardABIConfig.ERC223TokenABI;

        /// <summary>
        /// Creates new instance of the contract object
        /// </summary>
        /// <param name="game">Game that manages these items</param>
        /// <param name="web3">web3 interface</param>
        /// <param name="address">ethereum address of the contract</param>
        public ERC223GameItemContract(GameID game, Web3 web3, string address) : base(game, web3, address, ABI) { }
        /// <summary>
        /// Creates new instance of the contract object
        /// </summary>
        /// <param name="game">Game that manages these items</param>
        /// <param name="web3">web3 interface</param>
        /// <param name="address">ethereum address of the contract</param>
        /// <param name="abi">new ABI to override</param>
        public ERC223GameItemContract(GameID game, Web3 web3, string address, string abi) : base(game, web3, address, abi) { }

        private Function GetFunctionTransfer()
        {
            return contract.GetFunction("transfer");
        }

        private Function GetFunctionTokenState()
        {
            return contract.GetFunction("tokenState");
        }

        /// <summary>
        /// Returns state of this Item
        /// </summary>
        /// <returns></returns>
        public Task<byte[]> GetTokenState()
        {
            var function = GetFunctionTokenState();
            return function.CallAsync<byte[]>();
        }

        /// <inheritdoc/>
        public override async Task<bool> Transfer(Profile from, string addressTo, GameItem item, BigInteger amount)
        {
            var function = GetFunctionTransfer();
            object[] functionInput = { addressTo.RemoveHexPrefix(), amount };
            var receipt = await Hoard.BC.BCComm.EvaluateOnBC(web3, from, function, functionInput);
            return receipt.Status.Value == 1;
        }

        /// <inheritdoc/>
        public override async Task<GameItem[]> GetGameItems(Profile profile)
        {
            BigInteger itemBalance = await GetBalanceOf(profile.ID);
            if (BigInteger.Zero.CompareTo(itemBalance)<0)
            {
                Metadata meta = new Metadata(Address, itemBalance);
                GameItem gi = new GameItem(Game, await GetSymbol(), meta);
                gi.State = await GetTokenState();
                return new GameItem[] { gi };
            }
            else
                return new GameItem[0];
        }

        /// <inheritdoc/>
        public override async Task<GameItem[]> GetGameItems(Profile profile, ulong page = 0, ulong itemsPerPage = 0)
        {
            return await GetGameItems(profile);
        }

        /// <inheritdoc/>
        public override async Task<GameItem> GetGameItem(GameItemsParams gameItemsParams)
        {
            Metadata meta = new Metadata(Address, gameItemsParams.Amount);
            var item = new GameItem(Game, await GetSymbol(), meta);
            item.State = await GetTokenState();
            return item;
        }
    }

    /// <summary>
    /// ERC721 Game Item contract
    /// </summary>
    public class ERC721GameItemContract : GameItemContract
    {
        /// <summary>
        /// Inner data of ERC721 Game Item (for internal use)
        /// </summary>
        public class Metadata : BaseGameItemMetadata
        {
            /// <summary>
            /// Owner of the item
            /// </summary>
            public string OwnerAddress { get; set; }
            /// <summary>
            /// Identifier of the item
            /// </summary>
            public BigInteger ItemId { get; set; }

            /// <summary>
            /// Creates new instance of the metadata object
            /// </summary>
            /// <param name="ownerAddress">Owner address</param>
            /// <param name="itemID">Identifier of Item</param>
            public Metadata(string ownerAddress, BigInteger itemID)
            {
                OwnerAddress = ownerAddress;
                ItemId = itemID;
            }
        }

        /// <summary>
        /// ERC721GameItem interface identifier
        /// </summary>
        public const string InterfaceID = "0x80ac58cd";

        /// <summary>
        /// base ABI of this contract
        /// </summary>
        public const string ABI = HoardABIConfig.ERC721TokenABI;

        /// <summary>
        /// Creates new instance of ERC721 GameItem contract
        /// </summary>
        /// <param name="game">Managing game</param>
        /// <param name="web3">web3 interface</param>
        /// <param name="address">contract address</param>
        public ERC721GameItemContract(GameID game, Web3 web3, string address) : base(game, web3, address, ABI) { }

        /// <summary>
        /// Creates new instance of ERC721 GameItem contract
        /// </summary>
        /// <param name="game">Managing game</param>
        /// <param name="web3">web3 interface</param>
        /// <param name="address">contract address</param>
        /// <param name="abi">new ABI to override</param>
        public ERC721GameItemContract(GameID game, Web3 web3, string address, string abi) : base(game, web3, address, abi) { }

        private Function GetFunctionTransfer()
        {
            return contract.GetFunction("safeTransferFrom");
        }

        private Function GetFunctionTokenState()
        {
            return contract.GetFunction("tokenState");
        }

        private Function GetFunctionTokenStateArray()
        {
            return contract.GetFunction("tokenStateArray");
        }

        private Function GetFunctionTokenOfOwnerByIndex()
        {
            return contract.GetFunction("tokenOfOwnerByIndex");
        }

        private Function GetFunctionTokensOfOwnerByIndices()
        {
            return contract.GetFunction("tokensOfOwnerByIndices");
        }

        private Function GetFunctionExists()
        {
            return contract.GetFunction("exists");
        }

        /// <summary>
        /// Returns state of a particular token
        /// </summary>
        /// <param name="itemID">identifier of the item</param>
        /// <returns></returns>
        public Task<byte[]> GetTokenState(BigInteger itemID)
        {
            Function function = GetFunctionTokenState();
            return function.CallAsync<byte[]>(itemID);
        }

        /// <summary>
        /// Returns states of a particular pack of tokens
        /// </summary>
        /// <param name="itemIDs">identifiers of the items</param>
        /// <returns></returns>
        public Task<List<byte[]>> GetTokenStateArray(BigInteger[] itemIDs)
        {
            Function function = GetFunctionTokenStateArray();
            return function.CallAsync<List<byte[]>>(itemIDs);
        }

        /// <summary>
        /// Checks if item with given identifier exists
        /// </summary>
        /// <param name="itemID">identifier of the item</param>
        /// <returns></returns>
        public Task<bool> Exists(BigInteger itemID)
        {
            Function function = GetFunctionExists();
            return function.CallAsync<bool>(itemID);
        }

        /// <summary>
        /// Returns Item owned by owner by its ordinal number
        /// </summary>
        /// <param name="owner">owner address of the item</param>
        /// <param name="index">ordinal number</param>
        /// <returns></returns>
        public Task<BigInteger> TokenOfOwnerByIndex(string owner, ulong index)
        {
            Function function = GetFunctionTokenOfOwnerByIndex();
            return function.CallAsync<BigInteger>(owner, index);
        }

        /// <summary>
        /// Returns a pack of Items owned by owner by their ordinal number
        /// </summary>
        /// <param name="owner">owner address of the item</param>
        /// <param name="firstItemIndex">Start index for items pack</param>
        /// <param name="itemsToGather">Number of items to gather</param>
        /// <returns></returns>
        public Task<List<BigInteger>> TokensOfOwnerByIndices(string owner, BigInteger firstItemIndex, BigInteger itemsToGather)
        {
            Function function = GetFunctionTokensOfOwnerByIndices();
            return function.CallAsync<List<BigInteger>>(owner, firstItemIndex, itemsToGather);
        }

        /// <summary>
        /// Returns owner address of the item
        /// </summary>
        /// <param name="itemID">item identifier</param>
        /// <returns></returns>
        public Task<BigInteger> OwnerOf(BigInteger itemID)
        {
            Function function = contract.GetFunction("ownerOf");
            return function.CallAsync<BigInteger>(itemID);
        }

        /// <summary>
        /// Transfers <paramref name="amount"/> of <paramref name="item"/> from account <paramref name="from"/>
        /// to address <paramref name="addressTo"/>
        /// </summary>
        /// <param name="from">Account of items owner</param>
        /// <param name="addressTo">address of destination account</param>
        /// <param name="item">item to transfer</param>
        /// <param name="amount">amount of items to transfer</param>
        /// <returns></returns>
        public override async Task<bool> Transfer(Profile from, string addressTo, GameItem item, BigInteger amount)
        {
            var function = GetFunctionTransfer();
            BigInteger tokenId = (item.Metadata as ERC721GameItemContract.Metadata).ItemId;
            object[] functionInput = { from.ID.ToString(), addressTo.RemoveHexPrefix(), tokenId };
            var receipt = await BCComm.EvaluateOnBC(web3, from, function, functionInput);
            return receipt.Status.Value == 1;
        }

        /// <summary>
        /// Returns all items owned by <paramref name="profile"/>
        /// </summary>
        /// <param name="profile">Owners's profile</param>
        /// <returns></returns>
        public override async Task<GameItem[]> GetGameItems(Profile profile)
        {
            BigInteger itemBalance = await GetBalanceOf(profile.ID);
            string symbol = await GetSymbol();

            ulong count = (ulong)itemBalance;

            GameItem[] items = new GameItem[count];

            for (ulong i = 0; i < count; ++i)
            {
                BigInteger id = await TokenOfOwnerByIndex(profile.ID, i);
                Metadata meta = new Metadata(Address, id);

                items[i] = new GameItem(Game, symbol, meta);
                items[i].State = await GetTokenState(id);
            }

            return items;
        }

        /// <summary>
        /// Returns a pack of items owned by <paramref name="profile"/>
        /// </summary>
        /// <param name="profile">Owners's profile</param>
        /// <param name="page">Page number</param>
        /// <param name="itemsPerPage">Number of items per page</param>
        /// <returns></returns>
        public override async Task<GameItem[]> GetGameItems(Profile profile, ulong page, ulong itemsPerPage)
        {
            BigInteger itemBalance = await GetBalanceOf(profile.ID);
            BigInteger count = itemBalance;
            BigInteger firstItemIndex = page * itemsPerPage;
            if (firstItemIndex >= itemBalance)
                return new GameItem[0];
            List<BigInteger> ids = await TokensOfOwnerByIndices(profile.ID, firstItemIndex, itemsPerPage);
            string symbol = await GetSymbol();
            GameItem[] items = new GameItem[ids.Count];
            List<byte[]> states = await GetTokenStateArray(ids.ToArray());
            Debug.Assert(states.Count == ids.Count);
            for (int i = 0; i < ids.Count; i++)
            {
                Metadata meta = new Metadata(Address, ids[i]);
                items[i] = new GameItem(Game, symbol, meta);
                items[i].State = states[i];
            }
            return items;
        }

        /// <summary>
        /// Returns particular Item based on query parameters
        /// </summary>
        /// <param name="gameItemsParams">query parameters</param>
        /// <returns></returns>
        public override async Task<GameItem> GetGameItem(GameItemsParams gameItemsParams)
        {
            var id = BigInteger.Parse(gameItemsParams.TokenId, NumberStyles.AllowHexSpecifier);
            Metadata meta = new Metadata(Address, id);
            GameItem item = new GameItem(Game, await GetSymbol(), meta);
            item.State = await GetTokenState(id);
            return item;
        }
    }
}
