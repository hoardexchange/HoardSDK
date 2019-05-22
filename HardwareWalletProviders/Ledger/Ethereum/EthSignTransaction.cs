using Nethereum.Signer;
using System.IO;
using System.Linq;

namespace Hoard.HW.Ledger.Ethereum
{
    internal static class EthSignTransaction
    {
        public static byte[] Request(byte[] derivation, byte[] rlpTransactionChunk, bool firstBlock = true)
        {
            return APDU.InputData(EthConstants.CLA,
                                EthConstants.INS_SIGN_TRANSACTION,
                                firstBlock ? EthConstants.P1_FIRST_BLOCK : EthConstants.P1_SUBSEQUENT_BLOCK,
                                EthConstants.EMPTY,
                                firstBlock ? derivation.Concat(rlpTransactionChunk).ToArray() : rlpTransactionChunk);
        }

        public static string GetSignature(byte[] output)
        {
            using (var memory = new MemoryStream(output))
            {
                using (var reader = new BinaryReader(memory))
                {
                    byte[] sigV = reader.ReadBytes(1);
                    byte[] sigR = reader.ReadBytes(32);
                    byte[] sigS = reader.ReadBytes(32);

                    var signature = new EthECDSASignature(
                        new Org.BouncyCastle.Math.BigInteger(sigR), 
                        new Org.BouncyCastle.Math.BigInteger(sigS), 
                        sigV);
                    return EthECDSASignature.CreateStringSignature(signature);
                }
            }
        }
    }
}
