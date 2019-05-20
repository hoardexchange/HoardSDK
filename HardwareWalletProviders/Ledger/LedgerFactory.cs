using Device.Net;
using Hoard.HW.Ledger.Ethereum;
using System.Threading.Tasks;

namespace Hoard.HW.Ledger
{
    /// <summary>
    /// Factory class for acquiring instances of Ledger Hardware Wallets
    /// </summary>
    public static class LedgerFactory
    {
        private static FilterDeviceDefinition[] DeviceInfo = new FilterDeviceDefinition[]
        {
            new FilterDeviceDefinition{DeviceType= DeviceType.Usb, VendorId= 0x2c97, ProductId=0x0000, Label="HIDBlue" },
            new FilterDeviceDefinition{DeviceType= DeviceType.Usb, VendorId= 0x2c97, ProductId=0x0001, Label="HIDNanoS" },
            new FilterDeviceDefinition{DeviceType= DeviceType.Usb, VendorId= 0x2581, ProductId=0x3b7c, Label="WinHID" }
        };

        private static readonly UsageSpecification[] UsageSpecification = new[] 
        {
            new UsageSpecification(0xffa0, 0x01)
        };

        /// <summary>
        /// Creates a wallet instance based on derivationPath (name)
        /// </summary>
        /// <param name="derivationPath">path to get specific wallet (usually DerivationPath.BIP44)</param>
        /// <returns></returns>
        public static async Task<LedgerWallet> GetLedgerWalletAsync(string derivationPath)
        {
            var hidDevice = await Helpers.GetHIDDeviceAsync(DeviceInfo, UsageSpecification);
            if (hidDevice != null)
            {
                return new EthLedgerWallet(hidDevice, derivationPath);
            }
            return null;
        }
    }
}
