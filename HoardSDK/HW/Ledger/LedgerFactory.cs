﻿using Hid.Net;
using Hoard.HW.Ledger.Ethereum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.HW.Ledger
{
    public static class LedgerFactory
    {
        public static DeviceInfo[] DeviceInfo = new DeviceInfo[]
        {
            new DeviceInfo(0x2c97, 0x0000, "HIDBlue"),
            new DeviceInfo(0x2c97, 0x0001, "HIDNanoS"),
            new DeviceInfo(0x2581, 0x3b7c, "WinHID")
        };

        private static readonly UsageSpecification[] UsageSpecification = new[] 
        {
            new UsageSpecification(0xffa0, 0x01)
        };

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