using Hoard.HW.Trezor.Ethereum;
using System.Threading.Tasks;

namespace Hoard.HW.Trezor
{
    public static class TrezorFactory
    {
        public static DeviceInfo[] DeviceInfo = new DeviceInfo[]
        {
            new DeviceInfo(0x534c, 0x0001, "HIDOne"),
            new DeviceInfo(0x1209, 0x53C1, "HIDModelT")
        };

        private static readonly UsageSpecification[] UsageSpecification = new[]
        {
            new UsageSpecification(0xff00, 0x01)
        };

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
