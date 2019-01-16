using Nethereum.Hex.HexConvertors.Extensions;

namespace Hoard.HW.Trezor.Ethereum
{
    [ProtoBuf.ProtoContract()]
    internal class EthSignMessageRequest : ProtoBuf.IExtensible
    {
        private ProtoBuf.IExtension __pbn__extensionData;
        ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoBuf.ProtoMember(1, Name = @"address_n")]
        public uint[] AddressNs { get; set; }

        [ProtoBuf.ProtoMember(2, Name = @"message", IsRequired = true)]
        public byte[] Message { get; set; }

    }

    [ProtoBuf.ProtoContract()]
    internal class EthSignMessageResponse : ProtoBuf.IExtensible
    {
        private ProtoBuf.IExtension __pbn__extensionData;
        ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoBuf.ProtoMember(1, Name = @"address")]
        public byte[] Address { get; set; }
        public bool ShouldSerializeAddress() => Address != null;
        public void ResetAddress() => Address = null;

        [ProtoBuf.ProtoMember(2, Name = @"signature")]
        public byte[] Signature { get; set; }
        public bool ShouldSerializeSignature() => Signature != null;
        public void ResetSignature() => Signature = null;
    }

    internal static class EthSignMessage
    {
        public static object Request(uint[] indices, byte[] message)
        {
            var request = new EthSignMessageRequest
            {
                Message = message,
                AddressNs = indices,
            };

            return request;
        }

        public static string GetRLPEncoded(object output, byte[] rlpEncodedTransaction)
        {
            if (output is EthSignMessageResponse)
            {
                var response = output as EthSignMessageResponse;
                return response.Signature.ToHex();
            }

            return null;
        }
    }
}
