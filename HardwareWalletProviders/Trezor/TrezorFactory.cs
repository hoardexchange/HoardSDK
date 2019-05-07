using Hoard.HW.Trezor.Ethereum;
using System.Threading.Tasks;

namespace Hoard.HW.Trezor
{
    /// <summary>
    /// Factory class for acquiring instances of Trezor Hardware Wallets
    /// </summary>
    public static class TrezorFactory
    {
        private static Device.Net.FilterDeviceDefinition[] DeviceInfo = new Device.Net.FilterDeviceDefinition[]
        {
            new Device.Net.FilterDeviceDefinition{ DeviceType = Device.Net.DeviceType.Hid, VendorId= 0x534C, ProductId=0x0001, Label="Trezor One Firmware 1.6.x" },
            new Device.Net.FilterDeviceDefinition{ DeviceType = Device.Net.DeviceType.Hid, VendorId= 0x1209, ProductId=0x53C1, Label="Trezor One Firmware 1.7.x" },
            new Device.Net.FilterDeviceDefinition{ DeviceType = Device.Net.DeviceType.Hid, VendorId= 0x1209, ProductId=0x53C0, Label="Model T" }
        };

        private static readonly UsageSpecification[] UsageSpecification = new[]
        {
            new UsageSpecification(0xff00, 0x01)
        };
        
        /// <summary>
        /// Creates instance of Trezor Wallet based on the derivation path
        /// </summary>
        /// <param name="derivationPath">Indicates which wallet to get (use this in form of URL)</param>
        /// <param name="pinInputProvider">user input provider used to get PIN or password from user</param>
        /// <returns></returns>
        public static async Task<TrezorWallet> GetTrezorWalletAsync(string derivationPath, IUserInputProvider pinInputProvider = null)
        {
            var hidDevice = await Helpers.GetHIDDeviceAsync(DeviceInfo, UsageSpecification);
            if (hidDevice != null)
            {
                var wallet = new EthTrezorWallet(hidDevice, derivationPath, pinInputProvider);
                await wallet.InitializeAsync();
                return wallet;
            }
            return null;
        }
    }
}
