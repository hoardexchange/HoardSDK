using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.BC.Plasma
{
    /// <summary>
    /// Plasma transaction builder
    /// </summary>
    public class TransactionBuilder
    {
        /// <summary>
        /// Builds the simplest erc223 transaction (one erc223 currency, one receiver, one sender)
        /// </summary>
        /// <param name="from">account of tokens owner</param>
        /// <param name="addressTo">address of destination account</param>
        /// <param name="inputUtxos">input utxos satisfing given amount</param>
        /// <param name="amount">amount of tokens to transfer</param>
        /// <returns>RLP encoded signed transaction</returns>
        public async Task<string> BuildERC223Transaction(AccountInfo from, string addressTo, List<ERC223UTXOData> inputUtxos, BigInteger amount)
        {
            Debug.Assert(inputUtxos != null);

            var tx = new Transaction();
            var sum = BigInteger.Zero;

            foreach (var inputUtxo in inputUtxos)
            {
                sum += inputUtxo.Amount;
                tx.AddInput(inputUtxo);
            }

            tx.AddOutput(addressTo, amount);
            if(sum > amount)
            {
                tx.AddOutput(from.ID, sum - amount);
            }

            var signedTransaction = await tx.Sign(from);

            //Transaction.NULL_SIGNATURE.HexToByteArray();
            return tx.GetRLPEncoded(new List<string>(){ signedTransaction });
        }

        /// <summary>
        /// Builds the simplest erc721 transaction (one erc721 currency, one item, one receiver, one sender)
        /// </summary>
        /// <param name="from">account of tokens owner</param>
        /// <param name="addressTo">address of destination account</param>
        /// <param name="inputUtxos">input utxos containing given token id</param>
        /// <param name="tokenId">id of token to transfer</param>
        /// <returns>RLP encoded signed transaction</returns>
        public async Task<string> BuildERC721Transaction(AccountInfo from, string addressTo, List<ERC721UTXOData> inputUtxos, BigInteger tokenId)
        {
            var erc721Utxos = inputUtxos.OfType<ERC721UTXOData>().ToList();
            Debug.Assert(erc721Utxos.Count() == inputUtxos.Count);

            var erc721Utxo = (erc721Utxos.Where(x => x.TokenIds.Contains(tokenId)).Select(x => x)).FirstOrDefault();
            if (erc721Utxo != null)
            {
                var tx = new Transaction();
                tx.AddInput(erc721Utxo);
                tx.AddOutput(addressTo, 0);
                var signedTransaction = await tx.Sign(from);

                return tx.GetRLPEncoded(new List<string>() { signedTransaction });
            }

            return null;
        }
    }
}
