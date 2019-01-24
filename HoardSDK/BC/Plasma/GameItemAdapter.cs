using Hoard.BC.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.BC.Plasma
{
    /// <summary>
    /// Base Hoard game item adapter (extends game item contract behaviour to use childchain)
    /// </summary>
    public abstract class GameItemAdapter
    {
        /// <summary>
        /// Game that manages this game item adapter
        /// </summary>
        protected GameID game { get; private set; }

        /// <summary>
        /// Ethereum game item contract contract
        /// </summary>
        protected GameItemContract contract;

        /// <summary>
        /// Communication channel with child chain (plasma)
        /// </summary>
        protected PlasmaComm plasmaComm;

        /// <summary>
        /// Returns symbol of this item (type of the item)
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetSymbol()
        {
            return await contract.GetSymbol();
        }

        /// <summary>
        /// Constructor of base game item adapter
        /// </summary>
        /// <param name="_plasmaComm">plasma communication</param>
        /// <param name="_game">game that manages these items</param>
        /// <param name="_contract">ethereum game item contract</param>
        public GameItemAdapter(PlasmaComm _plasmaComm, GameID _game, GameItemContract _contract)
        {
            contract = _contract;
            game = _game;
            plasmaComm = _plasmaComm;
        }

        /// <summary>
        /// Transfers <paramref name="item"/> from account <paramref name="from"/> to <paramref name="addressTo"/>
        /// in given <paramref name="amount"/>
        /// </summary>
        /// <param name="from">Account of the sender</param>
        /// <param name="addressTo">destination account address</param>
        /// <param name="item">Item to transfer</param>
        /// <param name="amount">Amount of items to transfer</param>
        /// <returns></returns>
        public abstract Task<bool> Transfer(AccountInfo from, string addressTo, GameItem item, BigInteger amount);

        /// <summary>
        /// Returns all Game Items owned by Account <paramref name="info"/>
        /// </summary>
        /// <param name="info">Owner account</param>
        /// <returns></returns>
        public abstract Task<GameItem[]> GetGameItems(HoardID info);

        /// <summary>
        /// Returns total amount of items given account owns
        /// </summary>
        /// <param name="address">Account address of the owner</param>
        /// <returns></returns>
        public abstract Task<BigInteger> GetBalanceOf(HoardID address);
    }

    /// <summary>
    /// ERC223 game item adapter using childchain
    /// </summary>
    public class ERC223GameItemAdapter : GameItemAdapter
    {
        private TransactionBuilder txBuilder = new TransactionBuilder();

        /// <summary>
        /// Constructor of erc223 game item adapter
        /// </summary>
        /// <param name="_plasmaComm">plasma communication</param>
        /// <param name="_game">game that manages these items</param>
        /// <param name="_contract">ethereum game item contract</param>
        public ERC223GameItemAdapter(PlasmaComm _plasmaComm, GameID _game, GameItemContract _contract)
            : base(_plasmaComm, _game, _contract)
        {
        }

        /// <inheritdoc/>
        public override async Task<bool> Transfer(AccountInfo from, string addressTo, GameItem gameItem, BigInteger amount)
        {
            Debug.Assert(gameItem.Metadata is ERC223GameItemContract.Metadata);

            if (gameItem.Metadata is ERC223GameItemContract.Metadata)
            {
                var metadata = gameItem.Metadata as ERC223GameItemContract.Metadata;

                var currencyAddress = metadata.OwnerAddress;

                var utxos = await plasmaComm.GetUtxos(from.ID, currencyAddress);
                var inputUtxos = ERC223UTXOData.FindInputs(utxos, amount);
                if (inputUtxos != null)
                {
                    var signedTransaction = await txBuilder.BuildERC223Transaction(from, addressTo, inputUtxos, amount);
                    return await plasmaComm.SubmitTransaction(signedTransaction);
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public override async Task<GameItem[]> GetGameItems(HoardID address)
        {
            var items = new List<GameItem>();
            var tokensData = await plasmaComm.GetTokensData(address, contract.Address);

            string symbol = await contract.GetSymbol();
            string state = BitConverter.ToString(await (contract as ERC223GameItemContract).GetTokenState());
            foreach (var tokenData in tokensData)
            {
                ERC223GameItemContract.Metadata meta = new ERC223GameItemContract.Metadata(state, contract.Address, tokenData.Amount);
                items.Add(new GameItem(game, symbol, meta));
            }

            return items.ToArray();
        }

        /// <inheritdoc/>
        public override async Task<BigInteger> GetBalanceOf(HoardID address)
        {
            var tokensData = await plasmaComm.GetTokensData(address, contract.Address);
            return tokensData.Count;
        }
    }

    /// <summary>
    /// ERC721 game item adapter using childchain
    /// </summary>
    public class ERC721GameItemAdapter : GameItemAdapter
    {
        private TransactionBuilder txBuilder = new TransactionBuilder();

        /// <summary>
        /// Constructor of erc721 game item adapter
        /// </summary>
        /// <param name="_plasmaComm">plasma communication</param>
        /// <param name="_game">game that manages these items</param>
        /// <param name="_contract">ethereum game item contract</param>
        public ERC721GameItemAdapter(PlasmaComm _plasmaComm, GameID _game, GameItemContract _contract)
            : base(_plasmaComm, _game, _contract)
        {
        }

        /// <inheritdoc/>
        public override async Task<bool> Transfer(AccountInfo from, string addressTo, GameItem gameItem, BigInteger amount)
        {
            Debug.Assert(gameItem.Metadata is ERC721GameItemContract.Metadata);

            var tokenId = (gameItem.Metadata as ERC721GameItemContract.Metadata).ItemId;
            var currencyAddress = (gameItem.Metadata as ERC721GameItemContract.Metadata).OwnerAddress;

            var utxos = await plasmaComm.GetUtxos(from.ID, currencyAddress);
            var erc721Utxos = utxos.OfType<ERC721UTXOData>().ToList();

            if (erc721Utxos != null)
            {
                var signedTransaction = await txBuilder.BuildERC721Transaction(from, addressTo, erc721Utxos, tokenId);
                return await plasmaComm.SubmitTransaction(signedTransaction);
            }

            return false;
        }

        /// <inheritdoc/>
        public override async Task<GameItem[]> GetGameItems(HoardID address)
        {
            var items = new List<GameItem>();
            var tokensData = await plasmaComm.GetTokensData(address, contract.Address);

            string symbol = await contract.GetSymbol();
            for (int i = 0; i < tokensData.Count; ++i)
            {
                ERC721GameItemContract.Metadata meta = new ERC721GameItemContract.Metadata(contract.Address, tokensData[i].TokenId);
                GameItem item = new GameItem(game, symbol, meta);
                
                item.State = await plasmaComm.GetTokenState(contract.Address, tokensData[i].TokenId);

                items.Add(item);
            }

            return items.ToArray();
        }

        /// <inheritdoc/>
        public override async Task<BigInteger> GetBalanceOf(HoardID address)
        {
            var tokensData = await plasmaComm.GetTokensData(address, contract.Address);
            return tokensData.Count;
        }
    }
}
