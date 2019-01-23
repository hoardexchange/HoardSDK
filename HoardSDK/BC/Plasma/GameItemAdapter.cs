using Hoard.BC.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.BC.Plasma
{
    public abstract class GameItemAdapter
    {
        /// <summary>
        /// Game that manages this Game Item
        /// </summary>
        protected GameID game { get; private set; }

        /// <summary>
        /// Ethereum contract
        /// </summary>
        protected GameItemContract contract;

        protected PlasmaComm plasmaComm;

        public async Task<string> GetSymbol()
        {
            return await contract.GetSymbol();
        }

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
        public abstract Task<GameItem[]> GetGameItems(AccountInfo info);
    }

    public class ERC223GameItemAdapter : GameItemAdapter
    {
        public ERC223GameItemAdapter(PlasmaComm _plasmaComm, GameID _game, GameItemContract _contract)
            : base(_plasmaComm, _game, _contract)
        {
        }

        public override async Task<bool> Transfer(AccountInfo from, string addressTo, GameItem gameItem, BigInteger amount)
        {
            Debug.Assert(gameItem.Metadata is ERC223GameItemContract.Metadata);

            if (gameItem.Metadata is ERC223GameItemContract.Metadata)
            {
                var metadata = gameItem.Metadata as ERC223GameItemContract.Metadata;

                var currencyAddress = metadata.OwnerAddress;

                var utxos = await plasmaComm.GetUtxos(from.ID, currencyAddress);
                var inputUtxos = ERC223UTXOData.FindInputs(from.ID, amount, utxos);
                if (inputUtxos != null)
                {
                    var tx = new Transaction();
                    inputUtxos.ForEach(x => tx.AddInput(x));
                    tx.AddOutput(addressTo, amount);
                    var signedTransaction = await tx.Sign(from);

                    //var encodedTransaction = await ERC223UTXOData.CreateTransaction(from, addressTo, amount, inputUtxos);
                    return await plasmaComm.SubmitTransaction(signedTransaction);
                }
            }

            return false;
        }

        public override async Task<GameItem[]> GetGameItems(AccountInfo account)
        {
            var items = new List<GameItem>();
            var tokensData = await plasmaComm.GetTokensData(account.ID, contract.Address);

            string symbol = await contract.GetSymbol();
            string state = BitConverter.ToString(await (contract as ERC223GameItemContract).GetTokenState());
            foreach (var tokenData in tokensData)
            {
                ERC223GameItemContract.Metadata meta = new ERC223GameItemContract.Metadata(state, contract.Address, tokenData.Amount);
                items.Add(new GameItem(game, symbol, meta));
            }

            return items.ToArray();
        }
    }

    public class ERC721GameItemAdapter : GameItemAdapter
    {
        public ERC721GameItemAdapter(PlasmaComm _plasmaComm, GameID _game, GameItemContract _contract)
            : base(_plasmaComm, _game, _contract)
        {
        }

        public override async Task<bool> Transfer(AccountInfo from, string addressTo, GameItem item, BigInteger amount)
        {
            var tokenId = (item.Metadata as ERC721GameItemContract.Metadata).ItemId;
            var currencyAddress = (item.Metadata as ERC721GameItemContract.Metadata).OwnerAddress;

            var utxos = await plasmaComm.GetUtxos(from.ID, currencyAddress);
            var erc721Utxos = utxos.OfType<ERC721UTXOData>().ToList();

            var erc721Utxo = (erc721Utxos.Where(x => x.TokenIds.Contains(tokenId)).Select(x => x)).FirstOrDefault();
            if (erc721Utxo != null)
            {
                var tx = new Transaction();
                tx.AddInput(erc721Utxo);
                tx.AddOutput(addressTo, tokenId);
                var signedTransaction = await tx.Sign(from);

                return await plasmaComm.SubmitTransaction(signedTransaction);
            }

            return false;
        }

        public override async Task<GameItem[]> GetGameItems(AccountInfo account)
        {
            var items = new List<GameItem>();
            var tokensData = await plasmaComm.GetTokensData(account.ID, contract.Address);

            string symbol = await contract.GetSymbol();
            for (int i = 0; i < tokensData.Count; ++i)
            {
                ERC721GameItemContract.Metadata meta = new ERC721GameItemContract.Metadata(contract.Address, tokensData[i].TokenId);
                GameItem item = new GameItem(game, symbol, meta);
                
                item.State = await plasmaComm.GetTokenState(account.ID, contract.Address, tokensData[i].TokenId);

                items.Add(item);
            }

            return items.ToArray();
        }
    }
}
