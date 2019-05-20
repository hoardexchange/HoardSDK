#pragma warning disable CS1591, CS0612, CS3021, IDE1006
using System.Text;

namespace Hoard.HW.Trezor.Ethereum
{
    [ProtoBuf.ProtoContract()]
    public class EthAddressRequest : ProtoBuf.IExtensible
    {
        private ProtoBuf.IExtension __pbn__extensionData;
        ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoBuf.ProtoMember(1, Name = @"address_n")]
        public uint[] AddressNs { get; set; }

        [ProtoBuf.ProtoMember(2, Name = @"show_display")]
        public bool ShowDisplay
        {
            get => __pbn__ShowDisplay.GetValueOrDefault();
            set => __pbn__ShowDisplay = value;
        }
        public bool ShouldSerializeShowDisplay() => __pbn__ShowDisplay != null;
        public void ResetShowDisplay() => __pbn__ShowDisplay = null;
        private bool? __pbn__ShowDisplay;

    }

    [ProtoBuf.ProtoContract()]
    public class EthAddressResponse : ProtoBuf.IExtensible
    {
        private ProtoBuf.IExtension __pbn__extensionData;
        ProtoBuf.IExtension ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)
            => ProtoBuf.Extensible.GetExtensionObject(ref __pbn__extensionData, createIfMissing);

        [ProtoBuf.ProtoMember(2, Name = @"address", IsRequired = true)]
        public string Address { get; set; }

    }

    public static class EthGetAddress
    {
        public static object Request(uint[] indices, bool display = false)
        {
            return new EthAddressRequest { AddressNs = indices, ShowDisplay = display };
        }

        public static string GetAddress(object output)
        {
            if(output is EthAddressResponse)
            {
                return (output as EthAddressResponse).Address;
            }

            return "";
        }
    }
}

#pragma warning restore CS1591, CS0612, CS3021, IDE1006