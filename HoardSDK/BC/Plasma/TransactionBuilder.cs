using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard.BC.Plasma
{
    /// <summary>
    /// Plasma transaction builder helper class
    /// </summary>
    public class TransactionBuilder
    {
        /// <summary>
        /// Builds the simplest, the most common ERC223 transaction (one ERC223 currency, one receiver, one sender)
        /// </summary>
        /// <param name="from">account of tokens owner</param>
        /// <param name="addressTo">address of destination account</param>
        /// <param name="inputUtxos">input utxos satisfing given amount</param>
        /// <param name="amount">amount of tokens to transfer</param>
        /// <returns>RLP encoded signed transaction</returns>
        public async Task<string> BuildERC223Transaction(AccountInfo from, string addressTo, List<ERC223UTXOData> inputUtxos, BigInteger amount)
        {
            Debug.Assert(inputUtxos != null);
            Debug.Assert(inputUtxos.Count() > 0);

            var tx = new Transaction();
            var sum = BigInteger.Zero;

            foreach (var inputUtxo in inputUtxos)
            {
                Debug.Assert(inputUtxos[0].Currency == inputUtxo.Currency);

                sum += inputUtxo.Amount;
                tx.AddInput(inputUtxo);
            }
            
            tx.AddOutput(new ERC223TransactionOutputData(addressTo, inputUtxos[0].Currency, amount));

#if TESUJI_PLASMA
            const UInt32 maxInputs = 4;
            const UInt32 maxOutputs = 4;

            Debug.Assert(tx.GetInputCount() <= maxInputs);
            for(UInt32 i = tx.GetInputCount(); i <= maxInputs; ++i)
            {
                tx.AddInput(UTXOData.Empty);
            }

            if (sum > amount)
            {
                tx.AddOutput(new ERC223TransactionOutputData(from.ID, inputUtxos[0].Currency, sum - amount));
            }

            for (UInt32 i = tx.GetOutputCount(); i <= maxOutputs; ++i)
            {
                tx.AddOutput(ERC223TransactionOutputData.Empty);
            }
#endif
            var signedTransaction = await tx.Sign(from);

            // FIXME: not sure if it won't change in Hoard version of Plasma
            return tx.GetRLPEncoded(new List<string>(){ signedTransaction, Transaction.NULL_SIGNATURE });
        }

        /// <summary>
        /// Builds the simplest, the most common ERC721 transaction (one ERC721 currency, one item, one receiver, one sender)
        /// </summary>
        /// <param name="from">account of tokens owner</param>
        /// <param name="addressTo">address of destination account</param>
        /// <param name="inputUtxos">input utxos containing given token id</param>
        /// <param name="tokenId">id of token to transfer</param>
        /// <returns>RLP encoded signed transaction</returns>
        public async Task<string> BuildERC721Transaction(AccountInfo from, string addressTo, List<ERC721UTXOData> inputUtxos, BigInteger tokenId)
        {
            Debug.Assert(inputUtxos != null);
            Debug.Assert(inputUtxos.Count() == 1);

            var erc721Utxo = (inputUtxos.Where(x => x.TokenIds.Contains(tokenId)).Select(x => x)).FirstOrDefault();
            if (erc721Utxo != null)
            {
                var tx = new Transaction();

                tx.AddInput(erc721Utxo);

                tx.AddOutput(new ERC721TransactionOutputData(addressTo, inputUtxos[0].Currency, new List<BigInteger>(){ tokenId }));

#if TESUJI_PLASMA
                const UInt32 maxInputs = 4;
                const UInt32 maxOutputs = 4;

                Debug.Assert(tx.GetInputCount() <= maxInputs);
                for (UInt32 i = tx.GetInputCount(); i <= maxInputs; ++i)
                {
                    tx.AddInput(UTXOData.Empty);
                }

                if (erc721Utxo.TokenIds.Count > 1)
                {
                    erc721Utxo.TokenIds.Remove(tokenId);
                    tx.AddOutput(new ERC721TransactionOutputData(from.ID, inputUtxos[0].Currency, erc721Utxo.TokenIds));
                }

                for (UInt32 i = tx.GetOutputCount(); i <= maxOutputs; ++i)
                {
                    tx.AddOutput(ERC721TransactionOutputData.Empty);
                }
#endif

                var signedTransaction = await tx.Sign(from);

                // FIXME: not sure if it won't change in Hoard version of Plasma
                return tx.GetRLPEncoded(new List<string>() { signedTransaction, Transaction.NULL_SIGNATURE });
            }

            return null;
        }
    }
}
