using Hoard.Utils;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;

namespace Hoard.HW.Trezor.Ethereum
{
    [ProtoBuf.ProtoContract()]
    public class EthSignTransactionRequest : ProtoBuf.IExtensible
    {
        private ProtoBuf.IExtension __pbn__extensionData;
        ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoBuf.ProtoMember(1, Name = @"address_n")]
        public uint[] AddressNs { get; set; }

        [ProtoBuf.ProtoMember(2, Name = @"nonce")]
        public byte[] Nonce { get; set; }
        public bool ShouldSerializeNonce() => Nonce != null;
        public void ResetNonce() => Nonce = null;

        [ProtoBuf.ProtoMember(3, Name = @"gas_price")]
        public byte[] GasPrice { get; set; }
        public bool ShouldSerializeGasPrice() => GasPrice != null;
        public void ResetGasPrice() => GasPrice = null;

        [ProtoBuf.ProtoMember(4, Name = @"gas_limit")]
        public byte[] GasLimit { get; set; }
        public bool ShouldSerializeGasLimit() => GasLimit != null;
        public void ResetGasLimit() => GasLimit = null;

        [ProtoBuf.ProtoMember(5, Name = @"to")]
        public byte[] To { get; set; }
        public bool ShouldSerializeTo() => To != null;
        public void ResetTo() => To = null;

        [ProtoBuf.ProtoMember(6, Name = @"value")]
        public byte[] Value { get; set; }
        public bool ShouldSerializeValue() => Value != null;
        public void ResetValue() => Value = null;

        [ProtoBuf.ProtoMember(7, Name = @"data_initial_chunk")]
        public byte[] DataInitialChunk { get; set; }
        public bool ShouldSerializeDataInitialChunk() => DataInitialChunk != null;
        public void ResetDataInitialChunk() => DataInitialChunk = null;

        [ProtoBuf.ProtoMember(8, Name = @"data_length")]
        public uint DataLength
        {
            get => __pbn__DataLength.GetValueOrDefault();
            set => __pbn__DataLength = value;
        }
        public bool ShouldSerializeDataLength() => __pbn__DataLength != null;
        public void ResetDataLength() => __pbn__DataLength = null;
        private uint? __pbn__DataLength;

        [ProtoBuf.ProtoMember(9, Name = @"chain_id")]
        public uint ChainId
        {
            get => __pbn__ChainId.GetValueOrDefault();
            set => __pbn__ChainId = value;
        }
        public bool ShouldSerializeChainId() => __pbn__ChainId != null;
        public void ResetChainId() => __pbn__ChainId = null;
        private uint? __pbn__ChainId;

        [ProtoBuf.ProtoMember(10, Name = @"tx_type")]
        public uint TxType
        {
            get => __pbn__TxType.GetValueOrDefault();
            set => __pbn__TxType = value;
        }
        public bool ShouldSerializeTxType() => __pbn__TxType != null;
        public void ResetTxType() => __pbn__TxType = null;
        private uint? __pbn__TxType;

    }

    [ProtoBuf.ProtoContract()]
    public class EthSignTransactionResponse : ProtoBuf.IExtensible
    {
        private ProtoBuf.IExtension __pbn__extensionData;
        ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoBuf.ProtoMember(1, Name = @"data_length")]
        public uint DataLength
        {
            get => __pbn__DataLength.GetValueOrDefault();
            set => __pbn__DataLength = value;
        }
        public bool ShouldSerializeDataLength() => __pbn__DataLength != null;
        public void ResetDataLength() => __pbn__DataLength = null;
        private uint? __pbn__DataLength;

        [ProtoBuf.ProtoMember(2, Name = @"signature_v")]
        public uint SignatureV
        {
            get => __pbn__SignatureV.GetValueOrDefault();
            set => __pbn__SignatureV = value;
        }
        public bool ShouldSerializeSignatureV() => __pbn__SignatureV != null;
        public void ResetSignatureV() => __pbn__SignatureV = null;
        private uint? __pbn__SignatureV;

        [ProtoBuf.ProtoMember(3, Name = @"signature_r")]
        public byte[] SignatureR { get; set; }
        public bool ShouldSerializeSignatureR() => SignatureR != null;
        public void ResetSignatureR() => SignatureR = null;

        [ProtoBuf.ProtoMember(4, Name = @"signature_s")]
        public byte[] SignatureS { get; set; }
        public bool ShouldSerializeSignatureS() => SignatureS != null;
        public void ResetSignatureS() => SignatureS = null;
    }

    public static class EthSignTransaction
    {
        public static object Request(uint[] indices, byte[] rlpEncodedTransaction)
        {
            var decodedList = RLP.Decode(rlpEncodedTransaction);
            var decodedRlpCollection = (RLPCollection)decodedList[0];
            var request = new EthSignTransactionRequest
            {
                Nonce = decodedRlpCollection[0].RLPData,
                GasPrice = decodedRlpCollection[1].RLPData,
                GasLimit = decodedRlpCollection[2].RLPData,
                To = decodedRlpCollection[3].RLPData,
                Value = decodedRlpCollection[4].RLPData,
                AddressNs = indices,
            };

            if (decodedRlpCollection.Count > 5 && decodedRlpCollection[5].RLPData != null)
            {
                request.DataInitialChunk = decodedRlpCollection[5].RLPData;
                request.DataLength = (uint)decodedRlpCollection[5].RLPData.Length;
            }

            return request;
        }

        public static string GetRLPEncoded(object output, byte[] rlpEncodedTransaction)
        {
            if (output is EthSignTransactionResponse)
            {
                var response = output as EthSignTransactionResponse;

                var decodedList = RLP.Decode(rlpEncodedTransaction);
                var decodedRlpCollection = (RLPCollection)decodedList[0];
                var data = decodedRlpCollection.ToBytes();
                var signer = new RLPSigner(data, response.SignatureR, response.SignatureS, (byte)response.SignatureV);
                return signer.GetRLPEncoded().ToHex();
            }

            return null;
        }
    }
}
