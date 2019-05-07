using Hoard.Utils;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;
using System.IO;
using System.Linq;

namespace Hoard.HW.Ledger.Ethereum
{
    internal static class EthSignMessage
    {
        public static byte[] Request(byte[] derivation, byte[] messageChunk, bool firstBlock = true)
        {
            return APDU.InputData(EthConstants.CLA,
                                EthConstants.INS_SIGN_PERSONAL_MESSAGE,
                                firstBlock ? EthConstants.P1_FIRST_BLOCK : EthConstants.P1_SUBSEQUENT_BLOCK,
                                EthConstants.EMPTY,
                                firstBlock ? derivation.Concat(messageChunk).ToArray() : messageChunk);
        }

        public static string GetStringSignature(byte[] signature)
        {
            using (var memory = new MemoryStream(signature))
            {
                using (var reader = new BinaryReader(memory))
                {
                    byte[] sigV = reader.ReadBytes(1);
                    byte[] sigR = reader.ReadBytes(32);
                    byte[] sigS = reader.ReadBytes(32);

                    return "0x" + sigR.ToHex().PadLeft(64, '0') + sigS.ToHex().PadLeft(64, '0') + sigV.ToHex();
                }
            }
        }
    }
}
