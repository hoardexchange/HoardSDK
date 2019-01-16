namespace Hoard.HW.Trezor
{
    [ProtoBuf.ProtoContract()]
    internal enum MessageType
    {
        [ProtoBuf.ProtoEnum(Name = @"MessageType_Initialize")]
        MessageTypeInitializeRequest = 0,
        [ProtoBuf.ProtoEnum(Name = @"MessageType_Failure")]
        MessageTypeFailure = 3,
        [ProtoBuf.ProtoEnum(Name = @"MessageType_Features")]
        MessageTypeFeatures = 17,
        [ProtoBuf.ProtoEnum(Name = @"MessageType_PinMatrixRequest")]
        MessageTypePinMatrixRequest = 18,
        [ProtoBuf.ProtoEnum(Name = @"MessageType_PinMatrixAck")]
        MessageTypePinMatrixAck = 19,
        [ProtoBuf.ProtoEnum(Name = @"MessageType_ButtonRequest")]
        MessageTypeButtonRequest = 26,
        [ProtoBuf.ProtoEnum(Name = @"MessageType_ButtonAck")]
        MessageTypeButtonAck = 27,
        [ProtoBuf.ProtoEnum(Name = @"MessageType_EthereumGetAddress")]
        MessageTypeEthAddressRequest = 56,
        [ProtoBuf.ProtoEnum(Name = @"MessageType_EthereumAddress")]
        MessageTypeEthAddressResponse = 57,
        [ProtoBuf.ProtoEnum(Name = @"MessageType_EthereumSignTx")]
        MessageTypeEthSignTransactionRequest = 58,
        [ProtoBuf.ProtoEnum(Name = @"MessageType_EthereumTxRequest")]
        MessageTypeEthSignTransactionResponse = 59,
        [ProtoBuf.ProtoEnum(Name = @"MessageType_EthereumSignMessage")]
        MessageTypeEthSignMessageRequest = 64,
        [ProtoBuf.ProtoEnum(Name = @"MessageType_EthereumMessageSignature")]
        MessageTypeEthSignMessageResponse= 66,
    }
}
