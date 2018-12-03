using Hid.Net;
using System;
using System.Threading.Tasks;

namespace Hoard.HW.Ledger.Ethereum
{
    public class EthLedgerWallet : LedgerWallet
    {
        private class HDWalletAccountInfo : AccountInfo
        {
            private EthLedgerWallet Wallet;

            public HDWalletAccountInfo(string name, string id, EthLedgerWallet wallet)
                :base(name,id)
            {
                Wallet = wallet;
            }

            public override async Task<string> SignMessage(byte[] input)
            {
                return await Wallet.SignMessage(input, this);
            }

            public override async Task<string> SignTransaction(byte[] input)
            {
                return await Wallet.SignTransaction(input, this);
            }
        }
        private KeyPath keyPath;
        private byte[] derivation;

        public EthLedgerWallet(IHidDevice hidDevice, string derivationPath, uint index = 0) : base(hidDevice, derivationPath)
        {
            keyPath = new KeyPath(derivationPath).Derive(index);
            derivation = keyPath.ToBytes();
        }

        public override async Task<bool> RequestAccounts(User user)
        {
            var output = await SendRequestAsync(EthGetAddress.Request(derivation));
            if(IsSuccess(output.StatusCode))
            {
                var address = EthGetAddress.GetAddress(output.Data);
                user.Accounts.Add(new HDWalletAccountInfo(AccountInfoName, address, this));
                return true;
            }

            return false;
        }

        public override async Task<string> SignTransaction(byte[] rlpEncodedTransaction, AccountInfo accountInfo)
        {
            var output = await SendRequestAsync(EthSignTransaction.Request(derivation, rlpEncodedTransaction));
            if(IsSuccess(output.StatusCode))
            {
                return EthSignTransaction.GetRLPEncoded(output.Data, rlpEncodedTransaction);
            }

            return null;
        }

        public override async Task<string> SignMessage(byte[] message, AccountInfo accountInfo)
        {
            throw new NotImplementedException();
        }
    }
}