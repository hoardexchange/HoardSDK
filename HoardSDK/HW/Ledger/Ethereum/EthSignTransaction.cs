using Hoard.Utils;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;
using System.IO;
using System.Linq;

namespace Hoard.HW.Ledger.Ethereum
{
    public static class EthSignTransaction
    {
        public static byte[] Request(byte[] derivation, byte[] rlpEncodedTransaction, bool firstBlock = true)
        {
            return APDU.InputData(EthConstants.CLA,
                                EthConstants.INS_SIGN_TRANSACTION,
                                firstBlock ? EthConstants.P1_FIRST_BLOCK : EthConstants.P1_SUBSEQUENT_BLOCK,
                                EthConstants.EMPTY,
                                derivation.Concat(rlpEncodedTransaction).ToArray());
        }

        public static string GetRLPEncoded(byte[] signature, byte[] rlpEncodedTransaction)
        {
            var decodedList = RLP.Decode(rlpEncodedTransaction);
            var decodedRlpCollection = (RLPCollection)decodedList[0];
            var data = decodedRlpCollection.ToBytes();

            using (var memory = new MemoryStream(signature))
            {
                using (var reader = new BinaryReader(memory))
                {
                    byte sigV = reader.ReadByte();
                    byte[] sigR = reader.ReadBytes(32);
                    byte[] sigS = reader.ReadBytes(32);
                    var signer = new RLPSigner(data, sigR, sigS, sigV);
                    return signer.GetRLPEncoded().ToHex();
                }
            }
        }
    }
}
