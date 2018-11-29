using System.IO;
using System.Text;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Hoard.HW.Ledger.Ethereum
{
    public static class EthGetAddress
    {
        public static byte[] Request(byte[] derivation, bool display = false, bool useChainCode = false)
        {
            return APDU.InputData(EthConstants.CLA, 
                                EthConstants.INS_GET_PUBLIC_ADDRESS,
                                display ? EthConstants.P1_CONFIRM : EthConstants.P1_NON_CONFIRM,
                                useChainCode ? EthConstants.P2_CHAINCODE : EthConstants.P2_NO_CHAINCODE,
                                derivation);
        }

        public static string GetAddress(byte[] output)
        {
            //if(output.ReturnCode) { }

            using (var memory = new MemoryStream(output))
            {
                using (var reader = new BinaryReader(memory))
                {
                    var publicKeyLength = reader.ReadByte();
                    var publicKeyData = reader.ReadBytes(publicKeyLength);

                    var addressLength = reader.ReadByte();
                    var addressData = reader.ReadBytes(addressLength);
                    
                    return "0x" + Encoding.ASCII.GetString(addressData).ToLower();
                }
            }
        }
    }
}

