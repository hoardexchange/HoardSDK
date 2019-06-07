using Hoard.BC.Contracts;
using Nethereum.Hex.HexConvertors.Extensions;
using PlasmaCore.RPC.OutputData;
using PlasmaCore.Transactions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        /// Transfers <paramref name="item"/> from account <paramref name="profileFrom"/> to <paramref name="addressTo"/>
        /// in given <paramref name="amount"/>
        /// </summary>
        /// <param name="profileFrom">Profile of the sender</param>
        /// <param name="addressTo">destination account address</param>
        /// <param name="item">Item to transfer</param>
        /// <param name="amount">Amount of items to transfer</param>
        /// <returns></returns>
        public abstract Task<bool> Transfer(Profile profileFrom, string addressTo, GameItem item, BigInteger amount);

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

        /// <summary>
        /// Deposits game item to root chain
        /// </summary>
        /// <param name="profileFrom">profile of the sender</param>
        /// <param name="gameItem">item to deposit</param>
        /// <returns></returns>
        public abstract Task<bool> Deposit(Profile profileFrom, GameItem gameItem);

        /// <summary>
        /// Starts standard withdrawal of a given game item
        /// </summary>
        /// <param name="profileFrom">profile of the sender</param>
        /// <param name="gameItem">item to withdraw</param>
        /// <returns></returns>
        public abstract Task<bool> StartExit(Profile profileFrom, GameItem gameItem);

        /// <summary>
        /// Processes game items withdrawal that have completed the challenge period
        /// </summary>
        /// <param name="profileFrom">profile of the sender</param>
        /// <returns></returns>
        public async Task<bool> ProcessExits(Profile profileFrom)
        {
            var receipt = await plasmaComm.ProcessExits(profileFrom, contract.Address, BigInteger.Zero, BigInteger.One);
            if (receipt != null && receipt.Status.Value == 1)
                return true;
            return false;
        }
    }

    /// <summary>
    /// ERC20 game item adapter using childchain
    /// </summary>
    public class ERC20GameItemAdapter : GameItemAdapter
    {
        /// <summary>
        /// Constructor of erc223 game item adapter
        /// </summary>
        /// <param name="_plasmaComm">plasma communication</param>
        /// <param name="_game">game that manages these items</param>
        /// <param name="_contract">ethereum game item contract</param>
        public ERC20GameItemAdapter(PlasmaComm _plasmaComm, GameID _game, GameItemContract _contract)
            : base(_plasmaComm, _game, _contract)
        {
        }

        /// <inheritdoc/>
        public override async Task<bool> Transfer(Profile profileFrom, string addressTo, GameItem gameItem, BigInteger amount)
        {
            Debug.Assert(gameItem.Metadata is ERC223GameItemContract.Metadata);

            if (gameItem.Metadata is ERC223GameItemContract.Metadata)
            {
                var metadata = gameItem.Metadata as ERC223GameItemContract.Metadata;

                var currencyAddress = metadata.OwnerAddress;

                var utxos = await plasmaComm.GetUtxos(profileFrom.ID, currencyAddress);
                if (utxos != null && utxos.Length > 0)
                {
                    var transaction = FCTransactionBuilder.Build(profileFrom.ID, addressTo, utxos, amount, currencyAddress);
                    string signedTransaction = await plasmaComm.SignTransaction(profileFrom, transaction);
                    var details = await plasmaComm.SubmitTransaction(signedTransaction);
                    return (details != null);
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public override async Task<GameItem[]> GetGameItems(HoardID address)
        {
            var items = new List<GameItem>();
            var balanceData = await plasmaComm.GetBalanceData(address, contract.Address);

            string symbol = await contract.GetSymbol();
            string state = BitConverter.ToString(await (contract as ERC223GameItemContract).GetTokenState());
            foreach (var data in balanceData)
            {
                ERC223GameItemContract.Metadata meta = new ERC223GameItemContract.Metadata(contract.Address, (data as FCBalanceData).Amount);
                items.Add(new GameItem(game, symbol, meta));
            }

            return items.ToArray();
        }

        /// <inheritdoc/>
        public override async Task<BigInteger> GetBalanceOf(HoardID address)
        {
            var balanceData = await plasmaComm.GetBalanceData(address, contract.Address);
            return balanceData.Length;
        }

        /// <inheritdoc/>
        public override async Task<bool> Deposit(Profile profileFrom, GameItem gameItem)
        {
            if (gameItem.Metadata is ERC223GameItemContract.Metadata)
            {
                var metadata = gameItem.Metadata as ERC223GameItemContract.Metadata;
                var receipt = await plasmaComm.Deposit(profileFrom, contract.Address, metadata.Balance);
                if (receipt != null && receipt.Status.Value == 1)
                    return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override async Task<bool> StartExit(Profile profileFrom, GameItem gameItem)
        {
            if (gameItem.Metadata is ERC223GameItemContract.Metadata)
            {
                var metadata = gameItem.Metadata as ERC223GameItemContract.Metadata;
                var utxo = await plasmaComm.GetUtxo(profileFrom.ID, contract.Address, metadata.Balance);
                if(utxo != null)
                {
                    var receipt = await plasmaComm.StartStandardExit(profileFrom, utxo);
                    if (receipt != null && receipt.Status.Value == 1)
                        return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// ERC721 game item adapter using childchain
    /// </summary>
    public class ERC721GameItemAdapter : GameItemAdapter
    {
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
        public override async Task<bool> Transfer(Profile profileFrom, string addressTo, GameItem gameItem, BigInteger amount)
        {
            throw new NotImplementedException();
            // TODO missing erc721 plasma implementation
            /*
            Debug.Assert(gameItem.Metadata is ERC721GameItemContract.Metadata);

            var tokenId = (gameItem.Metadata as ERC721GameItemContract.Metadata).ItemId;
            var currencyAddress = (gameItem.Metadata as ERC721GameItemContract.Metadata).OwnerAddress;

            var utxos = await plasmaComm.GetUtxos(profileFrom.ID, currencyAddress);
            if (utxos != null && utxos.Length > 0)
            {
                var transaction = await NFCTransactionBuilder.Build(profileFrom, addressTo, utxos, currency, tokenId);
                byte[] encodedTransaction = transaction.GetRLPEncoded();
                string signature = await profileFrom.SignTransaction(encodedTransaction);
                transaction.AddSignature(profileFrom.ID, signature);
                byte[] signedTransaction = transaction.GetSignedRLPEncoded();
                var details = await plasmaComm.SubmitTransaction(signedTransaction.ToHex());

            }
            return false;
            */
        }

        /// <inheritdoc/>
        public override async Task<GameItem[]> GetGameItems(HoardID address)
        {
            throw new NotImplementedException();

            // TODO missing erc721 plasma implementation
            /*
             * var items = new List<GameItem>();
            var balanceData = await plasmaComm.GetBalanceData(address, contract.Address);

            string symbol = await contract.GetSymbol();
            foreach(var data in balanceData)
            {
                ERC721GameItemContract.Metadata meta = new ERC721GameItemContract.Metadata(contract.Address, (data as NFCBalanceData).TokenId);
                GameItem item = new GameItem(game, symbol, meta);
                
                item.State = await plasmaComm.GetTokenState(contract.Address, (data as NFCBalanceData).TokenId);

                items.Add(item);
            }
            return items.ToArray();
            */
        }

        /// <inheritdoc/>
        public override async Task<BigInteger> GetBalanceOf(HoardID address)
        {
            var balanceData = await plasmaComm.GetBalanceData(address, contract.Address);
            return balanceData.Length;
        }

        /// <inheritdoc/>
        public override async Task<bool> Deposit(Profile profileFrom, GameItem gameItem)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override async Task<bool> StartExit(Profile profileFrom, GameItem gameItem)
        {
            throw new NotImplementedException();
        }
    }
}
