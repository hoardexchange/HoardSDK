using Hoard.HW.Ledger.Ethereum;
using System.Threading.Tasks;

namespace Hoard.HW.Ledger
{
    /// <summary>
    /// Factory class for acquiring instances of Ledger Hardware Wallets
    /// </summary>
    public static class LedgerFactory
    {
        private static DeviceInfo[] DeviceInfo = new DeviceInfo[]
        {
            new DeviceInfo(0x2c97, 0x0000, "HIDBlue"),
            new DeviceInfo(0x2c97, 0x0001, "HIDNanoS"),
            new DeviceInfo(0x2581, 0x3b7c, "WinHID")
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
